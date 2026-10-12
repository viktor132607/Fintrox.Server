namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserApiToken domain record; references checked in application layer.</summary>
public sealed class UserApiToken : OrganizationEntity
{
    public UserApiToken(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string tokenHash,
        string tokenLabel,
        DateTimeOffset expiresAtUtc,
        bool isRevoked)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        TokenHash = DomainRules.Hash(tokenHash, nameof(tokenHash));
        TokenLabel = DomainRules.Text(tokenLabel, nameof(tokenLabel));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
        IsRevoked = isRevoked;
    }

    public Guid UserId { get; }
    public string TokenHash { get; }
    public string TokenLabel { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public bool IsRevoked { get; }
}
