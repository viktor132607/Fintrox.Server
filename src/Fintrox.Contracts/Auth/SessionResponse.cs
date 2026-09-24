namespace Fintrox.Contracts.Auth;

public sealed record SessionResponse(
    Guid Id,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? RevokedAtUtc,
    string? CreatedByIp,
    string? UserAgent);
