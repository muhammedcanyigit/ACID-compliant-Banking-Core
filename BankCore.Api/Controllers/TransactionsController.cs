using System.Data;
using BankCore.Api.Auth;
using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;
using BankCore.Api.Fraud;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly BankDbContext _context;
    private readonly FraudCheckService _fraudCheckService;

    public TransactionsController(BankDbContext context, FraudCheckService fraudCheckService)
    {
        _context = context;
        _fraudCheckService = fraudCheckService;
    }

    // ACID'in dört harfinin de devrede olduğu tek endpoint:
    // Atomicity  -> ya iki hesap da güncellenir ya da hiçbiri (transaction + rollback)
    // Consistency-> yetersiz bakiyeyle bakiye asla negatife düşmez
    // Isolation  -> Serializable seviyesi, aynı hesaba aynı anda gelen iki transferin birbirine karışmasını engeller
    // Durability -> commit sonrası veri PostgreSQL'e kalıcı yazılır
    [HttpPost("transfer")]
    public async Task<ActionResult<TransactionResponse>> Transfer(TransferRequest request)
    {
        if (request.SenderAccountId == request.ReceiverAccountId)
            return BadRequest("Gönderen ve alıcı hesap aynı olamaz.");

        await using var dbTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var sender = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.SenderAccountId);
        var receiver = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.ReceiverAccountId);

        if (sender is null || receiver is null)
            return NotFound("Gönderen veya alıcı hesap bulunamadı.");

        // Bir müşteri sadece kendi hesabından para gönderebilir; Admin herhangi bir hesaptan gönderebilir.
        if (sender.CustomerId != User.GetCustomerId() && !User.IsAdmin())
            return Forbid();

        if (!sender.IsActive || !receiver.IsActive)
            return BadRequest("Hesaplardan biri aktif değil.");

        if (sender.Currency != receiver.Currency)
            return BadRequest("Farklı para birimleri arasında transfer şu an desteklenmiyor.");

        var flagReason = await _fraudCheckService.EvaluateAsync(request, sender, _context);
        if (flagReason is not null)
        {
            var flaggedTx = new Transaction
            {
                SenderAccountId = sender.Id,
                ReceiverAccountId = receiver.Id,
                Amount = request.Amount,
                Status = "Flagged",
                Description = request.Description,
                FlagReason = flagReason,
            };
            _context.Transactions.Add(flaggedTx);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return UnprocessableEntity(new { message = "İşlem şüpheli bulundu ve incelemeye alındı.", rule = flagReason });
        }

        if (sender.Balance < request.Amount)
        {
            var failedTx = new Transaction
            {
                SenderAccountId = sender.Id,
                ReceiverAccountId = receiver.Id,
                Amount = request.Amount,
                Status = "Failed",
                Description = request.Description,
            };
            _context.Transactions.Add(failedTx);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return BadRequest("Yetersiz bakiye.");
        }

        sender.Balance -= request.Amount;
        receiver.Balance += request.Amount;

        var transaction = new Transaction
        {
            SenderAccountId = sender.Id,
            ReceiverAccountId = receiver.Id,
            Amount = request.Amount,
            Status = "Completed",
            Description = request.Description,
        };
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();
        await dbTransaction.CommitAsync();

        return Ok(ToResponse(transaction));
    }

    // Bir işlemi sadece gönderen ya da alıcı hesabın sahibi (ya da Admin) görebilir.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionResponse>> GetById(Guid id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.SenderAccount)
            .Include(t => t.ReceiverAccount)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction is null) return NotFound();

        var customerId = User.GetCustomerId();
        var isParty = transaction.SenderAccount.CustomerId == customerId
                      || transaction.ReceiverAccount.CustomerId == customerId;

        if (!isParty && !User.IsAdmin())
            return Forbid();

        return ToResponse(transaction);
    }

    // Fraud analisti/Admin, kurallara takılan tüm işlemleri tek yerden görebilir.
    [HttpGet("flagged")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<TransactionResponse>>> GetFlagged()
    {
        var flagged = await _context.Transactions
            .Where(t => t.Status == "Flagged")
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return flagged.Select(ToResponse).ToList();
    }

    private static TransactionResponse ToResponse(Transaction t) =>
        new(t.Id, t.SenderAccountId, t.ReceiverAccountId, t.Amount, t.Status, t.Description, t.FlagReason, t.CreatedAt);
}
