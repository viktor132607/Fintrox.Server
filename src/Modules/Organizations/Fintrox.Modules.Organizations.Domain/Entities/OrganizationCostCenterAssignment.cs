namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationCostCenterAssignment record.</summary>
public sealed class OrganizationCostCenterAssignment : OrganizationEntity
{
    public OrganizationCostCenterAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessUnitId,
        Guid costCenterId,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessUnitId = DomainRules.Id(businessUnitId, nameof(businessUnitId));
        CostCenterId = DomainRules.Id(costCenterId, nameof(costCenterId));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public Guid BusinessUnitId { get; }
    public Guid CostCenterId { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
