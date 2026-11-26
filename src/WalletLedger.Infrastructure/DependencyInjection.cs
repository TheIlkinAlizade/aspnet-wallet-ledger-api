using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WalletLedger.Application.Auth;
using WalletLedger.Application.Accounts;
using WalletLedger.Application.Ledger;
using WalletLedger.Infrastructure.Accounts;
using WalletLedger.Infrastructure.Identity;
using WalletLedger.Infrastructure.Ledger;
using WalletLedger.Infrastructure.Persistence;

namespace WalletLedger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Wallet")
            ?? throw new InvalidOperationException("ConnectionStrings:Wallet is not configured.");

        services.AddDbContext<WalletDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton(TimeProvider.System);

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddIdentityCore<ApplicationUser>(ConfigureIdentity)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<WalletDbContext>();

        services.AddSingleton<TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IMoneyMovementService, MoneyMovementService>();
        
        return services;
    }

    private static void ConfigureIdentity(IdentityOptions options)
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    }
}