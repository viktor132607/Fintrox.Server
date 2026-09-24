using Fintrox.Contracts.Auth;

namespace Fintrox.Application.Identity;

public interface IAuthenticationService
{
    Task<AuthTokenResponse> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task<AuthTokenResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task<AuthTokenResponse> RefreshAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task RevokeAsync(
        RevokeRefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<CurrentUserResponse> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SessionResponse>> ListSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> RevokeSessionAsync(
        Guid userId,
        Guid sessionId,
        string? ipAddress,
        CancellationToken cancellationToken);
}
