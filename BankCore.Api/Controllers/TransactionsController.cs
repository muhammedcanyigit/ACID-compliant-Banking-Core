using System.Data;
using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly BankDbContext _context;

    public TransactionsController(BankDbContext context)
    {
        _context = context;
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

        if (!sender.IsActive || !receiver.IsActive)
            return BadRequest("Hesaplardan biri aktif değil.");

        if (sender.Currency != receiver.Currency)
            return BadRequest("Farklı para birimleri arasında transfer şu an desteklenmiyor.");

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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionResponse>> GetById(Guid id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction is null) return NotFound();
        return ToResponse(transaction);
    }

    private static TransactionResponse ToResponse(Transaction t) =>
        new(t.Id, t.SenderAccountId, t.ReceiverAccountId, t.Amount, t.Status, t.Description, t.CreatedAt);
}
