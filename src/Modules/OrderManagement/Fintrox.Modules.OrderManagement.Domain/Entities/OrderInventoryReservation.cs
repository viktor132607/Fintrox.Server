namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderInventoryReservation record.</summary>
public sealed class OrderInventoryReservation : OrganizationEntity
{
    public OrderInventoryReservation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentOrderId,
        Guid inventoryItemId,
        decimal quantity,
        Guid reservationReferenceId)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        ReservationReferenceId = DomainRules.Id(reservationReferenceId, nameof(reservationReferenceId));
    }

    public Guid FulfillmentOrderId { get; }
    public Guid InventoryItemId { get; }
    public decimal Quantity { get; }
    public Guid ReservationReferenceId { get; }
}
