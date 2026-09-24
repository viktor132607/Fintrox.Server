using System.Security.Cryptography;
using System.Text;
using Fintrox.Application.Authorization;
using Fintrox.Application.Identity;
using Fintrox.Application.Organizations;
using Fintrox.Contracts.Auth;
using Fintrox.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Identity;

public sealed class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    FintroxDbContext dbContext,
    IAccessTokenService accessTokenService,
    IOrganizationMembershipRepository memberships,
    TimeProvider timeProvider) : IAuthenticationService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<AuthTokenResponse> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var existing = await userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            throw new IdentityConflictException("An account with this email already exists.");
        }

        var now = timeProvider.GetUtcNow();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new IdentityConflictException(
                string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        return await IssueTokensAsync(user, ipAddress, userAgent, cancellationToken);
    }

    public async Task<AuthTokenResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null || !user.IsActive)
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            throw new AuthenticationException("The account is temporarily locked.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            throw new AuthenticationException("Invalid email or password.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var now = timeProvider.GetUtcNow();
        user.LastLoginAtUtc = now;
        user.UpdatedAtUtc = now;

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to update the user login state.");
        }

        return await IssueTokensAsync(user, ipAddress, userAgent, cancellationToken);
    }

    public async Task<AuthTokenResponse> RefreshAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var hash = HashToken(request.RefreshToken);

        var existing = await dbContext.RefreshTokens.SingleOrDefaultAsync(
            token => token.TokenHash == hash,
            cancellationToken);

        if (existing is null || !existing.IsActive(now))
        {
            throw new AuthenticationException("Invalid or expired refresh token.");
        }

        var user = await userManager.FindByIdAsync(existing.UserId.ToString());

        if (user is null || !user.IsActive)
        {
            throw new AuthenticationException("The user account is not available.");
        }

        var accessToken = accessTokenService.Create(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName);

        var (rawRefreshToken, replacement) = CreateRefreshToken(
            user.Id,
            ipAddress,
            userAgent,
            now);

        existing.RevokedAtUtc = now;
        existing.RevokedByIp = NormalizeMetadata(ipAddress, 64);
        existing.ReplacedByTokenId = replacement.Id;

        dbContext.RefreshTokens.Add(replacement);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            rawRefreshToken,
            replacement.ExpiresAtUtc);
    }

    public async Task RevokeAsync(
        RevokeRefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var hash = HashToken(request.RefreshToken);

        var token = await dbContext.RefreshTokens.SingleOrDefaultAsync(
            refreshToken => refreshToken.TokenHash == hash,
            cancellationToken);

        if (token is null || token.RevokedAtUtc is not null)
        {
            return;
        }

        token.RevokedAtUtc = timeProvider.GetUtcNow();
        token.RevokedByIp = NormalizeMetadata(ipAddress, 64);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null || !user.IsActive)
        {
            throw new AuthenticationException("The user account is not available.");
        }

        var userMemberships = await memberships.ListForUserAsync(userId, cancellationToken);
        var organizationIds = userMemberships.Select(x => x.OrganizationId).ToArray();

        var organizations = await dbContext.Organizations
            .AsNoTracking()
            .Where(x => organizationIds.Contains(x.Id) && x.IsActive)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var organizationResponses = userMemberships
            .Where(x => organizations.ContainsKey(x.OrganizationId))
            .Select(x =>
            {
                var organization = organizations[x.OrganizationId];

                return new UserOrganizationResponse(
                    organization.Id,
                    organization.Name,
                    organization.Slug,
                    x.Role.ToString(),
                    RolePermissions.For(x.Role));
            })
            .ToArray();

        return new CurrentUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.EmailConfirmed,
            user.TwoFactorEnabled,
            organizationResponses);
    }

    public async Task<IReadOnlyList<SessionResponse>> ListSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var tokens = await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId)
            .OrderByDescending(token => token.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);

        return tokens
            .Select(token => new SessionResponse(
                token.Id,
                token.CreatedAtUtc,
                token.ExpiresAtUtc,
                token.RevokedAtUtc,
                token.CreatedByIp,
                token.UserAgent))
            .ToArray();
    }

    public async Task<bool> RevokeSessionAsync(
        Guid userId,
        Guid sessionId,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var token = await dbContext.RefreshTokens.SingleOrDefaultAsync(
            x => x.Id == sessionId && x.UserId == userId,
            cancellationToken);

        if (token is null)
        {
            return false;
        }

        if (token.RevokedAtUtc is null)
        {
            token.RevokedAtUtc = timeProvider.GetUtcNow();
            token.RevokedByIp = NormalizeMetadata(ipAddress, 64);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private async Task<AuthTokenResponse> IssueTokensAsync(
        ApplicationUser user,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var accessToken = accessTokenService.Create(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName);

        var (rawRefreshToken, refreshToken) = CreateRefreshToken(
            user.Id,
            ipAddress,
            userAgent,
            now);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            rawRefreshToken,
            refreshToken.ExpiresAtUtc);
    }

    private static (string RawToken, RefreshToken Entity) CreateRefreshToken(
        Guid userId,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset now)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return (
            rawToken,
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = HashToken(rawToken),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.Add(RefreshTokenLifetime),
                CreatedByIp = NormalizeMetadata(ipAddress, 64),
                UserAgent = NormalizeMetadata(userAgent, 512)
            });
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    private static string? NormalizeMetadata(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
