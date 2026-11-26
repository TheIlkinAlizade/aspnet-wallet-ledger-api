using System.Security.Claims;
using WalletLedger.Application.Auth;
using WalletLedger.Application.Exceptions;

namespace WalletLedger.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(WalletClaims.Subject)?.Value;
        return Guid.TryParse(value, out var userId) ? userId : throw new AuthenticationFailedException();
    }
}