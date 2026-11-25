using WalletLedger.Application.Auth;

namespace WalletLedger.Api.Contracts.Auth;

public sealed record TokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt)
{
    public static TokenResponse From(AuthTokens tokens) => new(
        tokens.AccessToken,
        tokens.AccessTokenExpiresAt,
        tokens.RefreshToken,
        tokens.RefreshTokenExpiresAt);
}