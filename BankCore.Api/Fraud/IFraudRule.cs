using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;

namespace BankCore.Api.Fraud;

public interface IFraudRule
{
    // Flagged olduğunda Transaction.FlagReason'a yazılacak kısa isim.
    string Name { get; }

    Task<bool> IsSuspiciousAsync(TransferRequest request, Account sender, BankDbContext context);
}
