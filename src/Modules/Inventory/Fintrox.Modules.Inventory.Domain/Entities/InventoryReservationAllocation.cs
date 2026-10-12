namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryReservationAllocation domain record; references checked in application layer.</summary>
public sealed class InventoryReservationAllocation : OrganizationEntity
{
    public InventoryReservationAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid stockReservationId,
        Guid inventoryLotId,
        decimal allocatedQuantity,
        DateTimeOffset allocatedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        StockReservationId = DomainRules.Id(stockReservationId, nameof(stockReservationId));
        InventoryLotId = DomainRules.Id(inventoryLotId, nameof(inventoryLotId));
        AllocatedQuantity = DomainRules.Positive(allocatedQuantity, nameof(allocatedQuantity));
        AllocatedAtUtc = DomainRules.Instant(allocatedAtUtc, nameof(allocatedAtUtc));
    }

    public Guid StockReservationId { get; }
    public Guid InventoryLotId { get; }
    public decimal AllocatedQuantity { get; }
    public DateTimeOffset AllocatedAtUtc { get; }
}
