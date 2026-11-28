using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WalletLedger.Infrastructure.Persistence;

namespace WalletLedger.Api.IntegrationTests.Infrastructure;

public static class LedgerAssertions
{
    public static async Task BooksBalanceAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WalletDbContext>();

        Assert.Equal(0L, await dbContext.LedgerEntries.SumAsync(entry => entry.AmountMinorUnits));
        Assert.Equal(0, await CountUnbalancedTransactionsAsync(dbContext));
        Assert.Equal(0, await CountDriftingAccountsAsync(dbContext));
    }

    private static Task<int> CountUnbalancedTransactionsAsync(WalletDbContext dbContext) =>
        dbContext.LedgerEntries
            .GroupBy(entry => entry.TransactionId)
            .Where(group => group.Sum(entry => entry.AmountMinorUnits) != 0)
            .CountAsync();

    private static Task<int> CountDriftingAccountsAsync(WalletDbContext dbContext) =>
        dbContext.Accounts
            .Where(account => account.BalanceMinorUnits !=
                dbContext.LedgerEntries
                    .Where(entry => entry.AccountId == account.Id)
                    .Sum(entry => entry.AmountMinorUnits))
            .CountAsync();
}