namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned ReturnShipment record.</summary>
public sealed class ReturnShipment : OrganizationEntity
{
    public ReturnShipment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid returnAuthorizationId,
        string carrierCode,
        string trackingNumber,
        DateOnly shippedOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ReturnAuthorizationId = DomainRules.Id(returnAuthorizationId, nameof(returnAuthorizationId));
        CarrierCode = DomainRules.Text(carrierCode, nameof(carrierCode));
        TrackingNumber = DomainRules.Text(trackingNumber, nameof(trackingNumber));
        ShippedOn = DomainRules.Date(shippedOn, nameof(shippedOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ReturnAuthorizationId { get; }
    public string CarrierCode { get; }
    public string TrackingNumber { get; }
    public DateOnly ShippedOn { get; }
    public string Status { get; }
}
