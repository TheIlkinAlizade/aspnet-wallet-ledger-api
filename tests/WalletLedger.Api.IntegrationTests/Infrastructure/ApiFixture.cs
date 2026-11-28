using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WalletLedger.Infrastructure.Persistence;

namespace WalletLedger.Api.IntegrationTests.Infrastructure;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private TestApiFactory? factory;

    public IServiceProvider Services => factory!.Services;

    public HttpClient CreateClient() => factory!.CreateClient();

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();
        factory = new TestApiFactory(postgres.GetConnectionString());
        await ApplyMigrationsAsync();
    }

    public async Task DisposeAsync()
    {
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        await postgres.DisposeAsync();
    }

    private async Task ApplyMigrationsAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}