namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditEventDigest record; cross-module references require application authorization.</summary>
public sealed class AuditEventDigest : OrganizationEntity
{
    public AuditEventDigest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid eventId,
        string eventType,
        DateTimeOffset occurredAtUtc,
        string payloadSha256,
        int sequenceNumber)
        : base(id, organizationId, createdAtUtc)
    {
        EventId = DomainRules.Id(eventId, nameof(eventId));
        EventType = DomainRules.Text(eventType, nameof(eventType));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        PayloadSha256 = DomainRules.Hash(payloadSha256, nameof(payloadSha256));
        SequenceNumber = DomainRules.PositiveInt(sequenceNumber, nameof(sequenceNumber));
    }

    public Guid EventId { get; }
    public string EventType { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string PayloadSha256 { get; }
    public int SequenceNumber { get; }
}
