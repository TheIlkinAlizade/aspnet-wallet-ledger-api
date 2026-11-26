using System.ComponentModel.DataAnnotations;

namespace WalletLedger.Api.Contracts.Accounts;

public sealed record AmountRequest
{
    [Required, MaxLength(32)]
    public string Amount { get; init; } = string.Empty;
}