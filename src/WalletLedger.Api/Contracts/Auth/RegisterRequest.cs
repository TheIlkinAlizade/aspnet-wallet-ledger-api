using System.ComponentModel.DataAnnotations;

namespace WalletLedger.Api.Contracts.Auth;

public sealed record RegisterRequest
{
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(10), MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}