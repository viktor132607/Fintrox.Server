namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned GoodsHandlingUnit; tenant scope and input fields validated at creation.</summary>
public sealed class GoodsHandlingUnit : OrganizationEntity
{
    public GoodsHandlingUnit(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string handlingUnitCode,
        string handlingUnitType,
        decimal grossWeight,
        bool isSealed)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        HandlingUnitCode = DomainRules.Text(handlingUnitCode, nameof(handlingUnitCode));
        HandlingUnitType = DomainRules.Text(handlingUnitType, nameof(handlingUnitType));
        GrossWeight = DomainRules.Nonnegative(grossWeight, nameof(grossWeight));
        IsSealed = isSealed;
    }

    public Guid WarehouseId { get; }
    public string HandlingUnitCode { get; }
    public string HandlingUnitType { get; }
    public decimal GrossWeight { get; }
    public bool IsSealed { get; }
}
