using Microsoft.EntityFrameworkCore;
using WalletLedger.Application.Exceptions;
using WalletLedger.Domain;
using WalletLedger.Infrastructure.Persistence;

namespace WalletLedger.Infrastructure.Accounts;

internal static class AccountQueries
{
    public static async Task<Account> GetOwnedAsync(
        this WalletDbContext dbContext, Guid userId, Guid accountId, CancellationToken cancellationToken) =>
        await dbContext.Accounts.FirstOrDefaultAsync(
            account => account.Id == accountId && account.OwnerUserId == userId,
            cancellationToken)
        ?? throw new NotFoundException("Account not found.");

    public static async Task<Account> GetTransferTargetAsync(
        this WalletDbContext dbContext, Guid accountId, CancellationToken cancellationToken) =>
        await dbContext.Accounts.FirstOrDefaultAsync(
            account => account.Id == accountId && account.Kind == AccountKind.User,
            cancellationToken)
        ?? throw new NotFoundException("Account not found.");
}