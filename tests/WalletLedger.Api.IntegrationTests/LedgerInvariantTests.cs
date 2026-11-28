using WalletLedger.Api.IntegrationTests.Infrastructure;

namespace WalletLedger.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class LedgerInvariantTests(ApiFixture fixture)
{
    [Fact]
    public async Task Ledger_AfterMixedMovements_IsBalancedEverywhere()
    {
        var alice = await TestUser.RegisterAsync(fixture);
        var bob = await TestUser.RegisterAsync(fixture);
        var aliceAccount = await alice.OpenFundedAccountAsync("250.00");
        var bobAccount = await bob.OpenAccountAsync();

        (await alice.TransferAsync(aliceAccount.Id, bobAccount.Id, "75.25")).EnsureSuccessStatusCode();
        (await bob.TransferAsync(bobAccount.Id, aliceAccount.Id, "10.00")).EnsureSuccessStatusCode();
        (await bob.WithdrawAsync(bobAccount.Id, "5.25")).EnsureSuccessStatusCode();

        Assert.Equal(18475, (await alice.GetAccountAsync(aliceAccount.Id)).BalanceMinorUnits);
        Assert.Equal(6000, (await bob.GetAccountAsync(bobAccount.Id)).BalanceMinorUnits);
        await LedgerAssertions.BooksBalanceAsync(fixture.Services);
    }

    [Fact]
    public async Task Ledger_AfterRejectedMovements_StaysBalanced()
    {
        var alice = await TestUser.RegisterAsync(fixture);
        var account = await alice.OpenFundedAccountAsync("20.00");

        await alice.WithdrawAsync(account.Id, "20.01");
        await alice.TransferAsync(account.Id, account.Id, "1.00");
        await alice.DepositAsync(account.Id, "0");

        Assert.Equal(2000, (await alice.GetAccountAsync(account.Id)).BalanceMinorUnits);
        await LedgerAssertions.BooksBalanceAsync(fixture.Services);
    }
}