using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;

namespace BankCore.Api.Fraud;

// Yeni açılmış bir hesaptan hemen büyük bir tutar göndermek, "aç-doldur-boşalt"
// tipi dolandırıcılık senaryolarının tipik imzasıdır.
public class NewAccountHighAmountRule : IFraudRule
{
    private static readonly TimeSpan AccountAgeThreshold = TimeSpan.FromHours(1);
    private const decimal AmountThreshold = 1_000m;

    public string Name => "NewAccountHighAmount";

    public Task<bool> IsSuspiciousAsync(TransferRequest request, Account sender, BankDbContext context)
    {
        var isNewAccount = DateTime.UtcNow - sender.CreatedAt < AccountAgeThreshold;
        var isHighAmount = request.Amount > AmountThreshold;
        return Task.FromResult(isNewAccount && isHighAmount);
    }
}
