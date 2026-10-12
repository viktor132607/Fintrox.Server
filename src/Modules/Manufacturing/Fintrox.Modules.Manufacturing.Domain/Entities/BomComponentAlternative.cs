namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned BomComponentAlternative record.</summary>
public sealed class BomComponentAlternative : OrganizationEntity
{
    public BomComponentAlternative(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid billOfMaterialsLineId,
        Guid alternativeInventoryItemId,
        decimal substitutionRatio,
        int priority)
        : base(id, organizationId, createdAtUtc)
    {
        BillOfMaterialsLineId = DomainRules.Id(billOfMaterialsLineId, nameof(billOfMaterialsLineId));
        AlternativeInventoryItemId = DomainRules.Id(alternativeInventoryItemId, nameof(alternativeInventoryItemId));
        SubstitutionRatio = DomainRules.Positive(substitutionRatio, nameof(substitutionRatio));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
    }

    public Guid BillOfMaterialsLineId { get; }
    public Guid AlternativeInventoryItemId { get; }
    public decimal SubstitutionRatio { get; }
    public int Priority { get; }
}
