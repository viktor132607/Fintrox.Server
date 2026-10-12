namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned SessionSecurityContext domain record; references checked in application layer.</summary>
public sealed class SessionSecurityContext : OrganizationEntity
{
    public SessionSecurityContext(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        Guid sessionReferenceId,
        string deviceFingerprint,
        string ipAddress,
        DateTimeOffset lastSeenAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        SessionReferenceId = DomainRules.Id(sessionReferenceId, nameof(sessionReferenceId));
        DeviceFingerprint = DomainRules.Text(deviceFingerprint, nameof(deviceFingerprint));
        IpAddress = DomainRules.Text(ipAddress, nameof(ipAddress));
        LastSeenAtUtc = DomainRules.Instant(lastSeenAtUtc, nameof(lastSeenAtUtc));
    }

    public Guid UserId { get; }
    public Guid SessionReferenceId { get; }
    public string DeviceFingerprint { get; }
    public string IpAddress { get; }
    public DateTimeOffset LastSeenAtUtc { get; }
}
