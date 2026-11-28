using System.Net.Http.Headers;
using System.Net.Http.Json;
using WalletLedger.Api.Contracts.Accounts;
using WalletLedger.Api.Contracts.Auth;

namespace WalletLedger.Api.IntegrationTests.Infrastructure;

public sealed class TestUser
{
    public const string Password = "Test-Passw0rd-Strong";

    private TestUser(HttpClient client, string email, TokenResponse tokens)
    {
        Client = client;
        Email = email;
        Tokens = tokens;
    }

    public HttpClient Client { get; }
    public string Email { get; }
    public TokenResponse Tokens { get; }

    public static async Task<TestUser> RegisterAsync(ApiFixture fixture)
    {
        var client = fixture.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@example.test";
        var credentials = new { email, password = Password };

        (await client.PostAsJsonAsync("/api/auth/register", credentials)).EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", credentials);
        login.EnsureSuccessStatusCode();
        var tokens = (await login.Content.ReadFromJsonAsync<TokenResponse>())!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return new TestUser(client, email, tokens);
    }

    public async Task<AccountResponse> OpenAccountAsync(string currency = "USD")
    {
        var response = await Client.PostAsJsonAsync("/api/accounts", new { currency });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountResponse>())!;
    }

    public async Task<AccountResponse> OpenFundedAccountAsync(string amount, string currency = "USD")
    {
        var account = await OpenAccountAsync(currency);
        (await DepositAsync(account.Id, amount)).EnsureSuccessStatusCode();
        return await GetAccountAsync(account.Id);
    }

    public async Task<AccountResponse> GetAccountAsync(Guid accountId)
    {
        var response = await Client.GetAsync($"/api/accounts/{accountId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountResponse>())!;
    }

    public Task<HttpResponseMessage> DepositAsync(Guid accountId, string amount) =>
        Client.PostAsJsonAsync($"/api/accounts/{accountId}/deposits", new { amount });

    public Task<HttpResponseMessage> WithdrawAsync(Guid accountId, string amount) =>
        Client.PostAsJsonAsync($"/api/accounts/{accountId}/withdrawals", new { amount });

    public Task<HttpResponseMessage> TransferAsync(Guid sourceAccountId, Guid destinationAccountId, string amount) =>
        Client.PostAsJsonAsync("/api/transfers", new { sourceAccountId, destinationAccountId, amount });
}