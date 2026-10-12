namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned LeadConversion record. Domain invariants enforced at construction.</summary>
public sealed class LeadConversion : OrganizationEntity
{
    public LeadConversion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid leadId,
        Guid counterpartyId,
        Guid opportunityId,
        Guid convertedByUserId,
        DateTimeOffset convertedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        LeadId = DomainRules.Id(leadId, nameof(leadId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        OpportunityId = DomainRules.Id(opportunityId, nameof(opportunityId));
        ConvertedByUserId = DomainRules.Id(convertedByUserId, nameof(convertedByUserId));
        ConvertedAtUtc = DomainRules.Instant(convertedAtUtc, nameof(convertedAtUtc));
    }

    public Guid LeadId { get; }
    public Guid CounterpartyId { get; }
    public Guid OpportunityId { get; }
    public Guid ConvertedByUserId { get; }
    public DateTimeOffset ConvertedAtUtc { get; }
}
