namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned OpportunityStageHistory record. Domain invariants enforced at construction.</summary>
public sealed class OpportunityStageHistory : OrganizationEntity
{
    public OpportunityStageHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid opportunityId,
        Guid fromStageId,
        Guid toStageId,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        OpportunityId = DomainRules.Id(opportunityId, nameof(opportunityId));
        FromStageId = DomainRules.Id(fromStageId, nameof(fromStageId));
        ToStageId = DomainRules.Id(toStageId, nameof(toStageId));
        ChangedByUserId = DomainRules.Id(changedByUserId, nameof(changedByUserId));
        ChangedAtUtc = DomainRules.Instant(changedAtUtc, nameof(changedAtUtc));
    }

    public Guid OpportunityId { get; }
    public Guid FromStageId { get; }
    public Guid ToStageId { get; }
    public Guid ChangedByUserId { get; }
    public DateTimeOffset ChangedAtUtc { get; }
}
