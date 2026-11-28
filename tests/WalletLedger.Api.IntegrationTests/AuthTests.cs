using System.Net;
using System.Net.Http.Json;
using WalletLedger.Api.Contracts.Auth;
using WalletLedger.Api.IntegrationTests.Infrastructure;

namespace WalletLedger.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AuthTests(ApiFixture fixture)
{
    [Fact]
    public async Task Me_AfterLogin_ReturnsUserRole()
    {
        var user = await TestUser.RegisterAsync(fixture);

        var me = await user.Client.GetFromJsonAsync<MeResponse>("/api/auth/me");

        Assert.Equal(new[] { "User" }, me!.Roles);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await fixture.CreateClient().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var user = await TestUser.RegisterAsync(fixture);

        var response = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/login", new { email = user.Email, password = "Wrong-Passw0rd-Value" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var response = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/login", new { email = "nobody@example.test", password = TestUser.Password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithExistingEmail_Returns409()
    {
        var user = await TestUser.RegisterAsync(fixture);

        var response = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/register", new { email = user.Email, password = TestUser.Password });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("alllowercase123")]
    [InlineData("NoDigitsInThisOne")]
    public async Task Register_WithWeakPassword_Returns400(string password)
    {
        var response = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/register", new { email = $"weak-{Guid.NewGuid():N}@example.test", password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_IssuesNewTokenPair()
    {
        var user = await TestUser.RegisterAsync(fixture);

        var response = await RefreshAsync(user.Tokens.RefreshToken);
        var rotated = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(user.Tokens.RefreshToken, rotated!.RefreshToken);
    }

    [Fact]
    public async Task Refresh_ReusingRotatedToken_RevokesWholeFamily()
    {
        var user = await TestUser.RegisterAsync(fixture);
        var first = await RefreshAsync(user.Tokens.RefreshToken);
        var rotated = (await first.Content.ReadFromJsonAsync<TokenResponse>())!;

        var reuse = await RefreshAsync(user.Tokens.RefreshToken);
        var afterRevocation = await RefreshAsync(rotated.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, afterRevocation.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var user = await TestUser.RegisterAsync(fixture);
        var anonymous = fixture.CreateClient();

        var logout = await anonymous.PostAsJsonAsync("/api/auth/logout", new { refreshToken = user.Tokens.RefreshToken });
        var refresh = await RefreshAsync(user.Tokens.RefreshToken);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    private Task<HttpResponseMessage> RefreshAsync(string refreshToken) =>
        fixture.CreateClient().PostAsJsonAsync("/api/auth/refresh", new { refreshToken });
}