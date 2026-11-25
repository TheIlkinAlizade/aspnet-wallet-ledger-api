using System.ComponentModel.DataAnnotations;

namespace WalletLedger.Api.Contracts.Auth;

public sealed record RefreshTokenRequest
{
    [Required, MaxLength(256)]
    public string RefreshToken { get; init; } = string.Empty;
}