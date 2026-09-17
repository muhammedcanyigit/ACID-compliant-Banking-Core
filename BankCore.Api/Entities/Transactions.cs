namespace BankCore.Api.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenderAccountId { get; set; }
    public Guid ReceiverAccountId { get; set; }
    
    // Transfer edilen tutar (decimal)
    public decimal Amount { get; set; } 
    
    // Status: Pending, Completed, Failed, Flagged
    public string Status { get; set; } = "Pending"; 
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties: İlişkili hesaplar
    public Account SenderAccount { get; set; } = null!;
    public Account ReceiverAccount { get; set; } = null!;
}