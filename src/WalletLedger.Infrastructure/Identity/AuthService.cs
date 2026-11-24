using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WalletLedger.Application.Auth;
using WalletLedger.Application.Exceptions;
using WalletLedger.Infrastructure.Persistence;

namespace WalletLedger.Infrastructure.Identity;

internal sealed record IssuedRefreshToken(Guid Id, string Plain, DateTimeOffset ExpiresAt);

internal sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    WalletDbContext dbContext,
    TokenService tokenService,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider) : IAuthService
{
    private readonly JwtOptions jwt = jwtOptions.Value;

    public async Task RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var user = new ApplicationUser { Id = Guid.CreateVersion7(), UserName = email, Email = email };
        EnsureSucceeded(await userManager.CreateAsync(user, password));
        EnsureSucceeded(await userManager.AddToRoleAsync(user, Roles.User));

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<AuthTokens> LoginAsync(
        string email, string password, string? ipAddress, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            throw new AuthenticationFailedException();
        }

        await VerifyPasswordAsync(user, password);
        return await IssueTokensAsync(user, Guid.CreateVersion7(), ipAddress, cancellationToken);
    }

    public async Task<AuthTokens> RefreshAsync(
        string refreshToken, string? ipAddress, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var stored = await FindStoredTokenAsync(refreshToken, cancellationToken);
        await EnsureUsableAsync(stored, now, cancellationToken);

        var user = await FindActiveUserAsync(stored.UserId);
        var issued = await StoreRefreshTokenAsync(user.Id, stored.FamilyId, ipAddress, now, cancellationToken);
        await RotateAsync(stored, issued.Id, now, cancellationToken);

        return await BuildTokensAsync(user, issued, now);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = TokenService.Hash(refreshToken);

        var familyId = await dbContext.RefreshTokens
            .Where(token => token.TokenHash == hash)
            .Select(token => (Guid?)token.FamilyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (familyId is null)
        {
            return;
        }

        await RevokeFamilyAsync(familyId.Value, timeProvider.GetUtcNow(), cancellationToken);
    }

    private async Task VerifyPasswordAsync(ApplicationUser user, string password)
    {
        if (await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.ResetAccessFailedCountAsync(user);
            return;
        }

        await userManager.AccessFailedAsync(user);
        throw new AuthenticationFailedException();
    }

    private async Task<AuthTokens> IssueTokensAsync(
        ApplicationUser user, Guid familyId, string? ipAddress, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var issued = await StoreRefreshTokenAsync(user.Id, familyId, ipAddress, now, cancellationToken);
        return await BuildTokensAsync(user, issued, now);
    }

    private async Task<AuthTokens> BuildTokensAsync(
        ApplicationUser user, IssuedRefreshToken issued, DateTimeOffset now)
    {
        var roles = await userManager.GetRolesAsync(user);
        var access = tokenService.CreateAccessToken(user.Id, roles, now);
        return new AuthTokens(access.Value, access.ExpiresAt, issued.Plain, issued.ExpiresAt);
    }

    private async Task<IssuedRefreshToken> StoreRefreshTokenAsync(
        Guid userId, Guid familyId, string? ipAddress, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var secret = tokenService.CreateRefreshTokenSecret();

        var entity = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = secret.Hash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(jwt.RefreshTokenDays),
            CreatedByIp = ipAddress
        };

        dbContext.RefreshTokens.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new IssuedRefreshToken(entity.Id, secret.Plain, entity.ExpiresAt);
    }

    private async Task<RefreshToken> FindStoredTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = TokenService.Hash(refreshToken);

        return await dbContext.RefreshTokens
                   .AsNoTracking()
                   .FirstOrDefaultAsync(token => token.TokenHash == hash, cancellationToken)
               ?? throw new AuthenticationFailedException();
    }

    private async Task EnsureUsableAsync(RefreshToken stored, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (stored.RevokedAt is not null)
        {
            await RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);
            throw new AuthenticationFailedException();
        }

        if (stored.ExpiresAt <= now)
        {
            throw new AuthenticationFailedException();
        }
    }

    private async Task<ApplicationUser> FindActiveUserAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            throw new AuthenticationFailedException();
        }

        return user;
    }

    private async Task RotateAsync(
        RefreshToken stored, Guid replacementId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var rotated = await dbContext.RefreshTokens
            .Where(token => token.Id == stored.Id && token.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(token => token.RevokedAt, now)
                    .SetProperty(token => token.ReplacedByTokenId, replacementId),
                cancellationToken);

        if (rotated == 0)
        {
            await RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);
            throw new AuthenticationFailedException();
        }
    }

    private Task<int> RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken cancellationToken) =>
        dbContext.RefreshTokens
            .Where(token => token.FamilyId == familyId && token.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.RevokedAt, now), cancellationToken);

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        if (result.Errors.Any(error => error.Code is "DuplicateUserName" or "DuplicateEmail"))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        throw new RequestValidationException(result.Errors.Select(error => error.Description).ToList());
    }
}