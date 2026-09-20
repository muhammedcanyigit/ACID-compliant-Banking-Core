using System.ComponentModel.DataAnnotations;

namespace BankCore.Api.DTOs;

public class CreateCustomerRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(11)]
    public string IdentityNumber { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public record CustomerResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    DateTime CreatedAt
);
