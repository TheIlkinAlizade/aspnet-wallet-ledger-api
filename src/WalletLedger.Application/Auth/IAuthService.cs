namespace WalletLedger.Application.Auth;

public interface IAuthService
{
    Task RegisterAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthTokens> LoginAsync(string email, string password, string? ipAddress, CancellationToken cancellationToken);
    Task<AuthTokens> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}