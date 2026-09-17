namespace BankCore.Api.Entities;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    // Default rol "Customer". İleride "Teller" veya "FraudAnalyst" atanabilir.
    public string Role { get; set; } = "Customer"; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property: Bir müşterinin birden fazla hesabı olabilir (1-N İlişki)
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}