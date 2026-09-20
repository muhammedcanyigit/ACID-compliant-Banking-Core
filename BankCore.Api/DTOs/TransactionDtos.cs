using System.ComponentModel.DataAnnotations;

namespace BankCore.Api.DTOs;

public class TransferRequest
{
    [Required]
    public Guid SenderAccountId { get; set; }

    [Required]
    public Guid ReceiverAccountId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public string? Description { get; set; }
}

public record TransactionResponse(
    Guid Id,
    Guid SenderAccountId,
    Guid ReceiverAccountId,
    decimal Amount,
    string Status,
    string? Description,
    DateTime CreatedAt
);
