namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned BinStockBalance; tenant scope and input fields validated at creation.</summary>
public sealed class BinStockBalance : OrganizationEntity
{
    public BinStockBalance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid binLocationId,
        Guid inventoryItemId,
        Guid inventoryLotId,
        decimal onHandQuantity,
        decimal reservedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        BinLocationId = DomainRules.Id(binLocationId, nameof(binLocationId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        InventoryLotId = DomainRules.Id(inventoryLotId, nameof(inventoryLotId));
        OnHandQuantity = DomainRules.Nonnegative(onHandQuantity, nameof(onHandQuantity));
        ReservedQuantity = DomainRules.Nonnegative(reservedQuantity, nameof(reservedQuantity));
    }

    public Guid BinLocationId { get; }
    public Guid InventoryItemId { get; }
    public Guid InventoryLotId { get; }
    public decimal OnHandQuantity { get; }
    public decimal ReservedQuantity { get; }
}
