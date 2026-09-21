using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Fraud;

// Aynı hesaptan kısa sürede art arda gelen çok sayıda transfer, çalıntı bir
// hesabın hızla boşaltılmaya çalışıldığının klasik belirtisidir.
public class VelocityRule : IFraudRule
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    private const int MaxTransfersInWindow = 3;

    public string Name => "Velocity";

    public async Task<bool> IsSuspiciousAsync(TransferRequest request, Account sender, BankDbContext context)
    {
        var since = DateTime.UtcNow - Window;
        var recentCount = await context.Transactions
            .CountAsync(t => t.SenderAccountId == sender.Id && t.CreatedAt >= since);

        return recentCount >= MaxTransfersInWindow;
    }
}
