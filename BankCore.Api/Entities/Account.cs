namespace BankCore.Api.Entities;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    
    // Finansal hassasiyet için float/double yerine decimal
    public decimal Balance { get; set; } = 0.00m; 
    public string Currency { get; set; } = "TRY";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property: Hesabın sahibi olan müşteri
    public Customer Customer { get; set; } = null!;

    // Navigation Properties: Bu hesaptan yapılan giden ve gelen transferler
    public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
    public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
}