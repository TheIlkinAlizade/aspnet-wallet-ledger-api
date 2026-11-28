using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WalletLedger.Api.IntegrationTests.Infrastructure;

internal sealed class TestApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Wallet", connectionString);
        builder.UseSetting("Jwt:Issuer", "wallet-ledger-tests");
        builder.UseSetting("Jwt:Audience", "wallet-ledger-tests");
        builder.UseSetting("Jwt:SigningKey", Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)));
    }
}