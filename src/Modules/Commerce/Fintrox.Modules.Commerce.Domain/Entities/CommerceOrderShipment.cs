namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned CommerceOrderShipment; external references validated by application layer.</summary>
public sealed class CommerceOrderShipment : OrganizationEntity
{
    public CommerceOrderShipment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid commerceOrderId,
        Guid shipmentId,
        string trackingReference,
        DateTimeOffset shippedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        CommerceOrderId = DomainRules.Id(commerceOrderId, nameof(commerceOrderId));
        ShipmentId = DomainRules.Id(shipmentId, nameof(shipmentId));
        TrackingReference = DomainRules.Text(trackingReference, nameof(trackingReference));
        ShippedAtUtc = DomainRules.Instant(shippedAtUtc, nameof(shippedAtUtc));
    }

    public Guid CommerceOrderId { get; }
    public Guid ShipmentId { get; }
    public string TrackingReference { get; }
    public DateTimeOffset ShippedAtUtc { get; }
}
