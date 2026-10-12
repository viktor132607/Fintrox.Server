namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplyChainInventoryProjection domain record. No cross-module navigation properties.</summary>
public sealed class SupplyChainInventoryProjection : OrganizationEntity
{
    public SupplyChainInventoryProjection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid warehouseId,
        DateOnly projectionOn,
        decimal projectedAvailableQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        ProjectionOn = DomainRules.Date(projectionOn, nameof(projectionOn));
        ProjectedAvailableQuantity = DomainRules.Nonnegative(projectedAvailableQuantity, nameof(projectedAvailableQuantity));
    }

    public Guid InventoryItemId { get; }
    public Guid WarehouseId { get; }
    public DateOnly ProjectionOn { get; }
    public decimal ProjectedAvailableQuantity { get; }
}
