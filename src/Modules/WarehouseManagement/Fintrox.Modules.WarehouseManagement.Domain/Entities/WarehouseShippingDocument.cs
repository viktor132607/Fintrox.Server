namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseShippingDocument; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseShippingDocument : OrganizationEntity
{
    public WarehouseShippingDocument(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid shipmentId,
        string documentType,
        Guid documentId,
        DateOnly issuedOn)
        : base(id, organizationId, createdAtUtc)
    {
        ShipmentId = DomainRules.Id(shipmentId, nameof(shipmentId));
        DocumentType = DomainRules.Text(documentType, nameof(documentType));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        IssuedOn = DomainRules.Date(issuedOn, nameof(issuedOn));
    }

    public Guid ShipmentId { get; }
    public string DocumentType { get; }
    public Guid DocumentId { get; }
    public DateOnly IssuedOn { get; }
}
