using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;

namespace BankCore.Api.Fraud;

// Tek seferde çok yüksek tutarlı transferler manuel incelemeye düşer.
public class HighAmountRule : IFraudRule
{
    private const decimal Threshold = 10_000m;

    public string Name => "HighAmount";

    public Task<bool> IsSuspiciousAsync(TransferRequest request, Account sender, BankDbContext context) =>
        Task.FromResult(request.Amount > Threshold);
}
