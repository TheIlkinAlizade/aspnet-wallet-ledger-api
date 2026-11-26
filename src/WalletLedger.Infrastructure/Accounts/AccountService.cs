using Microsoft.EntityFrameworkCore;
using WalletLedger.Application.Accounts;
using WalletLedger.Application.Exceptions;
using WalletLedger.Domain;
using WalletLedger.Infrastructure.Persistence;

namespace WalletLedger.Infrastructure.Accounts;

internal sealed class AccountService(WalletDbContext dbContext) : IAccountService
{
    private const int MaxAccountsPerUser = 10;

    public async Task<AccountView> OpenAsync(Guid userId, string currencyCode, CancellationToken cancellationToken)
    {
        var currency = Currency.Parse(currencyCode);
        await EnsureBelowLimitAsync(userId, cancellationToken);

        var account = Account.OpenForUser(userId, currency);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync(cancellationToken);

        return AccountView.From(account);
    }

    public async Task<IReadOnlyList<AccountView>> ListAsync(Guid userId, CancellationToken cancellationToken)
    {
        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.OwnerUserId == userId)
            .OrderBy(account => account.Id)
            .ToListAsync(cancellationToken);

        return accounts.Select(AccountView.From).ToList();
    }

    public async Task<AccountView> GetAsync(Guid userId, Guid accountId, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
                          .AsNoTracking()
                          .FirstOrDefaultAsync(
                              candidate => candidate.Id == accountId && candidate.OwnerUserId == userId,
                              cancellationToken)
                      ?? throw new NotFoundException("Account not found.");

        return AccountView.From(account);
    }

    private async Task EnsureBelowLimitAsync(Guid userId, CancellationToken cancellationToken)
    {
        var count = await dbContext.Accounts.CountAsync(account => account.OwnerUserId == userId, cancellationToken);

        if (count >= MaxAccountsPerUser)
        {
            throw new ConflictException($"An user can have at most {MaxAccountsPerUser} accounts.");
        }
    }
}