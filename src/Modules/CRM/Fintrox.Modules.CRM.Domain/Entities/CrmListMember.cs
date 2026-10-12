namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmListMember record. Domain invariants enforced at construction.</summary>
public sealed class CrmListMember : OrganizationEntity
{
    public CrmListMember(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid segmentId,
        Guid counterpartyId,
        DateTimeOffset addedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SegmentId = DomainRules.Id(segmentId, nameof(segmentId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        AddedAtUtc = DomainRules.Instant(addedAtUtc, nameof(addedAtUtc));
    }

    public Guid SegmentId { get; }
    public Guid CounterpartyId { get; }
    public DateTimeOffset AddedAtUtc { get; }
}
