namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationHierarchy record.</summary>
public sealed class OrganizationHierarchy : OrganizationEntity
{
    public OrganizationHierarchy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid parentUnitId,
        Guid childUnitId,
        string relationshipType,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        ParentUnitId = DomainRules.Id(parentUnitId, nameof(parentUnitId));
        ChildUnitId = DomainRules.Id(childUnitId, nameof(childUnitId));
        RelationshipType = DomainRules.Text(relationshipType, nameof(relationshipType));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid ParentUnitId { get; }
    public Guid ChildUnitId { get; }
    public string RelationshipType { get; }
    public DateOnly EffectiveFrom { get; }
}
