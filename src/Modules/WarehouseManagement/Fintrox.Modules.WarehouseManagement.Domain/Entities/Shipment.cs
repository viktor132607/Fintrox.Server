namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

public sealed class Shipment : OrganizationEntity
{
    public Shipment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        Guid fulfillmentOrderId,
        string reference)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        Reference = DomainRules.Text(reference, nameof(reference));
    }

    public Guid WarehouseId { get; }
    public Guid FulfillmentOrderId { get; }
    public string Reference { get; }
}
