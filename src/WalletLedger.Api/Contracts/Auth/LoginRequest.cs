using System.ComponentModel.DataAnnotations;

namespace WalletLedger.Api.Contracts.Auth;

public sealed record LoginRequest
{
    [Required, MaxLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}