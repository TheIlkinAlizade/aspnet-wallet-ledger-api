using Microsoft.AspNetCore.Authorization;
using WalletLedger.Application.Auth;

namespace WalletLedger.Api.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddWalletAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin));

        return services;
    }
}