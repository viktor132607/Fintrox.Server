namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserTrustedDevice domain record; references checked in application layer.</summary>
public sealed class UserTrustedDevice : OrganizationEntity
{
    public UserTrustedDevice(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string deviceFingerprint,
        DateTimeOffset firstSeenAtUtc,
        DateTimeOffset expiresAtUtc,
        bool isRevoked)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        DeviceFingerprint = DomainRules.Text(deviceFingerprint, nameof(deviceFingerprint));
        FirstSeenAtUtc = DomainRules.Instant(firstSeenAtUtc, nameof(firstSeenAtUtc));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
        IsRevoked = isRevoked;
    }

    public Guid UserId { get; }
    public string DeviceFingerprint { get; }
    public DateTimeOffset FirstSeenAtUtc { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public bool IsRevoked { get; }
}
