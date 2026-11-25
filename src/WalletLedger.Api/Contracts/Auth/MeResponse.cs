namespace WalletLedger.Api.Contracts.Auth;

public sealed record MeResponse(string? UserId, IReadOnlyList<string> Roles);