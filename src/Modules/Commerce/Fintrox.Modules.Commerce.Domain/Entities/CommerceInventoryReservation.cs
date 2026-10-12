namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned CommerceInventoryReservation; external references validated by application layer.</summary>
public sealed class CommerceInventoryReservation : OrganizationEntity
{
    public CommerceInventoryReservation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid commerceOrderId,
        Guid inventoryItemId,
        decimal quantity,
        DateTimeOffset reservedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        CommerceOrderId = DomainRules.Id(commerceOrderId, nameof(commerceOrderId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        ReservedAtUtc = DomainRules.Instant(reservedAtUtc, nameof(reservedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid CommerceOrderId { get; }
    public Guid InventoryItemId { get; }
    public decimal Quantity { get; }
    public DateTimeOffset ReservedAtUtc { get; }
    public string Status { get; }
}
