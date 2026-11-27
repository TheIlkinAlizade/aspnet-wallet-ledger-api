using System.ComponentModel.DataAnnotations;

namespace WalletLedger.Api.Contracts.Transfers;

public sealed record TransferRequest
{
    [Required]
    public Guid? SourceAccountId { get; init; }

    [Required]
    public Guid? DestinationAccountId { get; init; }

    [Required, MaxLength(32)]
    public string Amount { get; init; } = string.Empty;
}