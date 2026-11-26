using System.ComponentModel.DataAnnotations;

namespace WalletLedger.Api.Contracts.Accounts;

public sealed record OpenAccountRequest
{
    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; init; } = string.Empty;
}