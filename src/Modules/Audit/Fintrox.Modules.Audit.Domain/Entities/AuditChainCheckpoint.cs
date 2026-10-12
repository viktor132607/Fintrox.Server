namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditChainCheckpoint record; cross-module references require application authorization.</summary>
public sealed class AuditChainCheckpoint : OrganizationEntity
{
    public AuditChainCheckpoint(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid lastEventId,
        int sequenceNumber,
        string rootSha256,
        Guid createdByUserId,
        DateTimeOffset checkpointAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        LastEventId = DomainRules.Id(lastEventId, nameof(lastEventId));
        SequenceNumber = DomainRules.PositiveInt(sequenceNumber, nameof(sequenceNumber));
        RootSha256 = DomainRules.Hash(rootSha256, nameof(rootSha256));
        CreatedByUserId = DomainRules.Id(createdByUserId, nameof(createdByUserId));
        CheckpointAtUtc = DomainRules.Instant(checkpointAtUtc, nameof(checkpointAtUtc));
    }

    public Guid LastEventId { get; }
    public int SequenceNumber { get; }
    public string RootSha256 { get; }
    public Guid CreatedByUserId { get; }
    public DateTimeOffset CheckpointAtUtc { get; }
}
