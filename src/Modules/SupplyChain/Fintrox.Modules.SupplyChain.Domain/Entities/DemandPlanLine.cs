namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped DemandPlanLine domain record. No cross-module navigation properties.</summary>
public sealed class DemandPlanLine : OrganizationEntity
{
    public DemandPlanLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid demandPlanVersionId,
        Guid inventoryItemId,
        Guid warehouseId,
        DateOnly forecastOn,
        decimal forecastQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        DemandPlanVersionId = DomainRules.Id(demandPlanVersionId, nameof(demandPlanVersionId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        ForecastOn = DomainRules.Date(forecastOn, nameof(forecastOn));
        ForecastQuantity = DomainRules.Nonnegative(forecastQuantity, nameof(forecastQuantity));
    }

    public Guid DemandPlanVersionId { get; }
    public Guid InventoryItemId { get; }
    public Guid WarehouseId { get; }
    public DateOnly ForecastOn { get; }
    public decimal ForecastQuantity { get; }
}
