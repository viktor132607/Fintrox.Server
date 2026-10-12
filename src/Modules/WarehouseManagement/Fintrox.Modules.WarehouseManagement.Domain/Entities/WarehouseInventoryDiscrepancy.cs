namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseInventoryDiscrepancy; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseInventoryDiscrepancy : OrganizationEntity
{
    public WarehouseInventoryDiscrepancy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        Guid inventoryItemId,
        decimal discrepancyQuantity,
        DateOnly discoveredOn)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        DiscrepancyQuantity = DomainRules.Nonzero(discrepancyQuantity, nameof(discrepancyQuantity));
        DiscoveredOn = DomainRules.Date(discoveredOn, nameof(discoveredOn));
    }

    public Guid WarehouseId { get; }
    public Guid InventoryItemId { get; }
    public decimal DiscrepancyQuantity { get; }
    public DateOnly DiscoveredOn { get; }
}
