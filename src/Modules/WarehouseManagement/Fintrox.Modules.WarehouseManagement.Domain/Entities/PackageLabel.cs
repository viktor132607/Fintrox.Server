namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned PackageLabel; tenant scope and input fields validated at creation.</summary>
public sealed class PackageLabel : OrganizationEntity
{
    public PackageLabel(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid shipmentPackageId,
        string labelCode,
        string carrierCode,
        DateTimeOffset generatedAtUtc,
        Guid documentId)
        : base(id, organizationId, createdAtUtc)
    {
        ShipmentPackageId = DomainRules.Id(shipmentPackageId, nameof(shipmentPackageId));
        LabelCode = DomainRules.Text(labelCode, nameof(labelCode));
        CarrierCode = DomainRules.Text(carrierCode, nameof(carrierCode));
        GeneratedAtUtc = DomainRules.Instant(generatedAtUtc, nameof(generatedAtUtc));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
    }

    public Guid ShipmentPackageId { get; }
    public string LabelCode { get; }
    public string CarrierCode { get; }
    public DateTimeOffset GeneratedAtUtc { get; }
    public Guid DocumentId { get; }
}
