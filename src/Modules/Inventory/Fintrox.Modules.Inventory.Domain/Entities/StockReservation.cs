namespace Fintrox.Modules.Inventory.Domain.Entities;

public sealed class StockReservation : OrganizationEntity
{
    public StockReservation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        Guid warehouseId,
        Guid orderLineId,
        decimal quantity,
        DateTimeOffset expiresAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        OrderLineId = DomainRules.Id(orderLineId, nameof(orderLineId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
        if (ExpiresAtUtc <= CreatedAtUtc) throw new ArgumentException("Reservation must expire after creation.", nameof(expiresAtUtc));
    }

    public Guid ItemId { get; }
    public Guid WarehouseId { get; }
    public Guid OrderLineId { get; }
    public decimal Quantity { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
}
