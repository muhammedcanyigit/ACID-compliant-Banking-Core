using System.ComponentModel.DataAnnotations;

namespace BankCore.Api.DTOs;

public class CreateAccountRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    public string Currency { get; set; } = "TRY";
}

public record AccountResponse(
    Guid Id,
    Guid CustomerId,
    string AccountNumber,
    decimal Balance,
    string Currency,
    bool IsActive,
    DateTime CreatedAt
);
