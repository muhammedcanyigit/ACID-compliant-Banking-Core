using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly BankDbContext _context;

    public AccountsController(BankDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<AccountResponse>> Create(CreateAccountRequest request)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == request.CustomerId);
        if (!customerExists) return NotFound("Müşteri bulunamadı.");

        var account = new Account
        {
            CustomerId = request.CustomerId,
            AccountNumber = GenerateAccountNumber(),
            Currency = request.Currency,
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, ToResponse(account));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountResponse>> GetById(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account is null) return NotFound();
        return ToResponse(account);
    }

    private static string GenerateAccountNumber() =>
        Random.Shared.NextInt64(1_000_000_000, 9_999_999_999).ToString();

    private static AccountResponse ToResponse(Account a) =>
        new(a.Id, a.CustomerId, a.AccountNumber, a.Balance, a.Currency, a.IsActive, a.CreatedAt);
}
