namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplyChainTransportLeg domain record. No cross-module navigation properties.</summary>
public sealed class SupplyChainTransportLeg : OrganizationEntity
{
    public SupplyChainTransportLeg(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid shipmentId,
        string carrierCode,
        DateOnly departureOn,
        DateOnly arrivalOn,
        string trackingReference)
        : base(id, organizationId, createdAtUtc)
    {
        ShipmentId = DomainRules.Id(shipmentId, nameof(shipmentId));
        CarrierCode = DomainRules.Text(carrierCode, nameof(carrierCode));
        DepartureOn = DomainRules.Date(departureOn, nameof(departureOn));
        ArrivalOn = DomainRules.Date(arrivalOn, nameof(arrivalOn));
        TrackingReference = DomainRules.Text(trackingReference, nameof(trackingReference));
    }

    public Guid ShipmentId { get; }
    public string CarrierCode { get; }
    public DateOnly DepartureOn { get; }
    public DateOnly ArrivalOn { get; }
    public string TrackingReference { get; }
}
