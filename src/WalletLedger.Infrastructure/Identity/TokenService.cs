using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WalletLedger.Application.Auth;

namespace WalletLedger.Infrastructure.Identity;

internal sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

internal sealed record RefreshTokenSecret(string Plain, string Hash);

internal sealed class TokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions jwt = options.Value;
    private readonly JsonWebTokenHandler handler = new();

    public AccessToken CreateAccessToken(Guid userId, IEnumerable<string> roles, DateTimeOffset now)
    {
        var expiresAt = now.AddMinutes(jwt.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Subject = new ClaimsIdentity(BuildClaims(userId, roles)),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(jwt.CreateSigningKey(), SecurityAlgorithms.HmacSha256)
        };

        return new AccessToken(handler.CreateToken(descriptor), expiresAt);
    }

    public RefreshTokenSecret CreateRefreshTokenSecret()
    {
        var plain = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        return new RefreshTokenSecret(plain, Hash(plain));
    }

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static IEnumerable<Claim> BuildClaims(Guid userId, IEnumerable<string> roles)
    {
        yield return new Claim(WalletClaims.Subject, userId.ToString());
        yield return new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString());

        foreach (var role in roles)
        {
            yield return new Claim(WalletClaims.Role, role);
        }
    }
}