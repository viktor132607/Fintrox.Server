namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned OpportunityLossReason record. Domain invariants enforced at construction.</summary>
public sealed class OpportunityLossReason : OrganizationEntity
{
    public OpportunityLossReason(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid opportunityId,
        string reasonCode,
        string description,
        DateTimeOffset recordedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        OpportunityId = DomainRules.Id(opportunityId, nameof(opportunityId));
        ReasonCode = DomainRules.Text(reasonCode, nameof(reasonCode));
        Description = DomainRules.Text(description, nameof(description));
        RecordedAtUtc = DomainRules.Instant(recordedAtUtc, nameof(recordedAtUtc));
    }

    public Guid OpportunityId { get; }
    public string ReasonCode { get; }
    public string Description { get; }
    public DateTimeOffset RecordedAtUtc { get; }
}
