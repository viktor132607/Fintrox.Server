namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned ShipmentManifest; tenant scope and input fields validated at creation.</summary>
public sealed class ShipmentManifest : OrganizationEntity
{
    public ShipmentManifest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid shipmentId,
        string manifestNumber,
        string carrierCode,
        DateOnly createdOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ShipmentId = DomainRules.Id(shipmentId, nameof(shipmentId));
        ManifestNumber = DomainRules.Text(manifestNumber, nameof(manifestNumber));
        CarrierCode = DomainRules.Text(carrierCode, nameof(carrierCode));
        CreatedOn = DomainRules.Date(createdOn, nameof(createdOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ShipmentId { get; }
    public string ManifestNumber { get; }
    public string CarrierCode { get; }
    public DateOnly CreatedOn { get; }
    public string Status { get; }
}
