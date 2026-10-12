namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplyPlanLine domain record. No cross-module navigation properties.</summary>
public sealed class SupplyPlanLine : OrganizationEntity
{
    public SupplyPlanLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplyPlanVersionId,
        Guid inventoryItemId,
        Guid warehouseId,
        DateOnly neededOn,
        decimal plannedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        SupplyPlanVersionId = DomainRules.Id(supplyPlanVersionId, nameof(supplyPlanVersionId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        NeededOn = DomainRules.Date(neededOn, nameof(neededOn));
        PlannedQuantity = DomainRules.Nonnegative(plannedQuantity, nameof(plannedQuantity));
    }

    public Guid SupplyPlanVersionId { get; }
    public Guid InventoryItemId { get; }
    public Guid WarehouseId { get; }
    public DateOnly NeededOn { get; }
    public decimal PlannedQuantity { get; }
}
