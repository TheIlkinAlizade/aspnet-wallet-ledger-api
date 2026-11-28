using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WalletLedger.Api.Contracts.Transfers;
using WalletLedger.Api.IntegrationTests.Infrastructure;
using WalletLedger.Domain;
using WalletLedger.Infrastructure.Persistence;

namespace WalletLedger.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class TransferTests(ApiFixture fixture)
{
    [Fact]
    public async Task Transfer_WithEnoughFunds_MovesMoneyBetweenAccounts()
    {
        var (alice, bob, aliceAccount, bobAccount) = await SetUpPairAsync("100.00");

        var response = await alice.TransferAsync(aliceAccount.Id, bobAccount.Id, "30.00");
        var transfer = await response.Content.ReadFromJsonAsync<TransferResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Completed", transfer!.Status);
        Assert.Equal(7000, (await alice.GetAccountAsync(aliceAccount.Id)).BalanceMinorUnits);
        Assert.Equal(3000, (await bob.GetAccountAsync(bobAccount.Id)).BalanceMinorUnits);
    }

    [Fact]
    public async Task Transfer_AboveBalance_Returns422AndKeepsBalances()
    {
        var (alice, bob, aliceAccount, bobAccount) = await SetUpPairAsync("10.00");

        var response = await alice.TransferAsync(aliceAccount.Id, bobAccount.Id, "10.01");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(1000, (await alice.GetAccountAsync(aliceAccount.Id)).BalanceMinorUnits);
        Assert.Equal(0, (await bob.GetAccountAsync(bobAccount.Id)).BalanceMinorUnits);
    }

    [Fact]
    public async Task Transfer_ToSameAccount_Returns422()
    {
        var alice = await TestUser.RegisterAsync(fixture);
        var account = await alice.OpenFundedAccountAsync("50.00");

        var response = await alice.TransferAsync(account.Id, account.Id, "1.00");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(5000, (await alice.GetAccountAsync(account.Id)).BalanceMinorUnits);
    }

    [Fact]
    public async Task Transfer_ToAccountInOtherCurrency_Returns422()
    {
        var alice = await TestUser.RegisterAsync(fixture);
        var bob = await TestUser.RegisterAsync(fixture);
        var usdAccount = await alice.OpenFundedAccountAsync("50.00");
        var eurAccount = await bob.OpenAccountAsync("EUR");

        var response = await alice.TransferAsync(usdAccount.Id, eurAccount.Id, "5.00");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(5000, (await alice.GetAccountAsync(usdAccount.Id)).BalanceMinorUnits);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("10.005")]
    [InlineData("abc")]
    public async Task Transfer_WithInvalidAmount_Returns422(string amount)
    {
        var (alice, _, aliceAccount, bobAccount) = await SetUpPairAsync("100.00");

        var response = await alice.TransferAsync(aliceAccount.Id, bobAccount.Id, amount);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_IntoSystemClearingAccount_Returns404()
    {
        var (alice, _, aliceAccount, _) = await SetUpPairAsync("100.00");
        var clearingId = await FindClearingAccountIdAsync();

        var response = await alice.TransferAsync(aliceAccount.Id, clearingId, "1.00");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<(TestUser Alice, TestUser Bob, Contracts.Accounts.AccountResponse AliceAccount, Contracts.Accounts.AccountResponse BobAccount)>
        SetUpPairAsync(string aliceFunds)
    {
        var alice = await TestUser.RegisterAsync(fixture);
        var bob = await TestUser.RegisterAsync(fixture);
        var aliceAccount = await alice.OpenFundedAccountAsync(aliceFunds);
        var bobAccount = await bob.OpenAccountAsync();
        return (alice, bob, aliceAccount, bobAccount);
    }

    private async Task<Guid> FindClearingAccountIdAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WalletDbContext>();

        return await dbContext.Accounts
            .Where(account => account.Kind == AccountKind.SystemClearing && account.Currency == Currency.Usd)
            .Select(account => account.Id)
            .SingleAsync();
    }
}