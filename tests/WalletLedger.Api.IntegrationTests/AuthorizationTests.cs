using System.Net;
using System.Net.Http.Json;
using WalletLedger.Api.Contracts.Accounts;
using WalletLedger.Api.IntegrationTests.Infrastructure;

namespace WalletLedger.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AuthorizationTests(ApiFixture fixture)
{
    [Fact]
    public async Task GetAccount_OfAnotherUser_Returns404()
    {
        var victim = await TestUser.RegisterAsync(fixture);
        var attacker = await TestUser.RegisterAsync(fixture);
        var victimAccount = await victim.OpenFundedAccountAsync("100.00");

        var foreign = await attacker.Client.GetAsync($"/api/accounts/{victimAccount.Id}");
        var unknown = await attacker.Client.GetAsync($"/api/accounts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(unknown.StatusCode, foreign.StatusCode);
    }

    [Fact]
    public async Task Deposit_IntoAnotherUsersAccount_Returns404AndKeepsBalance()
    {
        var victim = await TestUser.RegisterAsync(fixture);
        var attacker = await TestUser.RegisterAsync(fixture);
        var victimAccount = await victim.OpenFundedAccountAsync("100.00");

        var response = await attacker.DepositAsync(victimAccount.Id, "50.00");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(10000, (await victim.GetAccountAsync(victimAccount.Id)).BalanceMinorUnits);
    }

    [Fact]
    public async Task Withdraw_FromAnotherUsersAccount_Returns404AndKeepsBalance()
    {
        var victim = await TestUser.RegisterAsync(fixture);
        var attacker = await TestUser.RegisterAsync(fixture);
        var victimAccount = await victim.OpenFundedAccountAsync("100.00");

        var response = await attacker.WithdrawAsync(victimAccount.Id, "50.00");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(10000, (await victim.GetAccountAsync(victimAccount.Id)).BalanceMinorUnits);
    }

    [Fact]
    public async Task Transfer_FromAnotherUsersAccount_Returns404AndKeepsBalance()
    {
        var victim = await TestUser.RegisterAsync(fixture);
        var attacker = await TestUser.RegisterAsync(fixture);
        var victimAccount = await victim.OpenFundedAccountAsync("100.00");
        var attackerAccount = await attacker.OpenAccountAsync();

        var response = await attacker.TransferAsync(victimAccount.Id, attackerAccount.Id, "50.00");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(10000, (await victim.GetAccountAsync(victimAccount.Id)).BalanceMinorUnits);
        Assert.Equal(0, (await attacker.GetAccountAsync(attackerAccount.Id)).BalanceMinorUnits);
    }

    [Fact]
    public async Task ListAccounts_ReturnsOnlyOwnAccounts()
    {
        var alice = await TestUser.RegisterAsync(fixture);
        var bob = await TestUser.RegisterAsync(fixture);
        var aliceAccount = await alice.OpenAccountAsync();
        await bob.OpenAccountAsync();

        var accounts = await alice.Client.GetFromJsonAsync<List<AccountResponse>>("/api/accounts");

        Assert.Equal([aliceAccount.Id], accounts!.Select(account => account.Id));
    }

    [Fact]
    public async Task OpenAccount_IgnoresClientSuppliedBalanceAndOwner()
    {
        var victim = await TestUser.RegisterAsync(fixture);
        var attacker = await TestUser.RegisterAsync(fixture);
        var victimAccount = await victim.OpenAccountAsync();

        var response = await attacker.Client.PostAsJsonAsync("/api/accounts", new
        {
            currency = "USD",
            balanceMinorUnits = 999_999,
            ownerUserId = victim.Email,
            id = victimAccount.Id
        });
        var created = await response.Content.ReadFromJsonAsync<AccountResponse>();
        var viaVictim = await victim.Client.GetAsync($"/api/accounts/{created!.Id}");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(0, created.BalanceMinorUnits);
        Assert.NotEqual(victimAccount.Id, created.Id);
        Assert.Equal(HttpStatusCode.NotFound, viaVictim.StatusCode);
    }

    [Theory]
    [InlineData("GET", "/api/accounts")]
    [InlineData("POST", "/api/accounts")]
    [InlineData("POST", "/api/transfers")]
    public async Task ProtectedEndpoints_WithoutToken_Return401(string method, string path)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path);

        var response = await fixture.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}