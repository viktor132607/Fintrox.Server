namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyRelationship. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyRelationship : OrganizationEntity
{
    public CounterpartyRelationship(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceCounterpartyId,
        Guid relatedCounterpartyId,
        string relationshipType,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        SourceCounterpartyId = DomainRules.Id(sourceCounterpartyId, nameof(sourceCounterpartyId));
        RelatedCounterpartyId = DomainRules.Id(relatedCounterpartyId, nameof(relatedCounterpartyId));
        RelationshipType = DomainRules.Text(relationshipType, nameof(relationshipType));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid SourceCounterpartyId { get; }
    public Guid RelatedCounterpartyId { get; }
    public string RelationshipType { get; }
    public DateOnly EffectiveFrom { get; }
}
