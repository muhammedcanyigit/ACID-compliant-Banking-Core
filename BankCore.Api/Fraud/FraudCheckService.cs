using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;

namespace BankCore.Api.Fraud;

public class FraudCheckService
{
    private readonly IEnumerable<IFraudRule> _rules;

    public FraudCheckService(IEnumerable<IFraudRule> rules)
    {
        _rules = rules;
    }

    // Kuralları sırayla dener, ilk tetiklenenin adını döner. Hiçbiri tetiklenmezse null.
    public async Task<string?> EvaluateAsync(TransferRequest request, Account sender, BankDbContext context)
    {
        foreach (var rule in _rules)
        {
            if (await rule.IsSuspiciousAsync(request, sender, context))
                return rule.Name;
        }

        return null;
    }
}
