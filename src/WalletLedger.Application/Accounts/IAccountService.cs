namespace WalletLedger.Application.Accounts;

public interface IAccountService
{
    Task<AccountView> OpenAsync(Guid userId, string currencyCode, CancellationToken cancellationToken);
    Task<IReadOnlyList<AccountView>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<AccountView> GetAsync(Guid userId, Guid accountId, CancellationToken cancellationToken);
}