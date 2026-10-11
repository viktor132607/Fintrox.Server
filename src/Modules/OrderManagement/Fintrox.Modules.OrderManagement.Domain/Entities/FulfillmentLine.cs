namespace Fintrox.Modules.OrderManagement.Domain.Entities;

public sealed class FulfillmentLine : OrganizationEntity
{
    public FulfillmentLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentOrderId,
        Guid salesOrderLineId,
        Guid warehouseId,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        SalesOrderLineId = DomainRules.Id(salesOrderLineId, nameof(salesOrderLineId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid FulfillmentOrderId { get; }
    public Guid SalesOrderLineId { get; }
    public Guid WarehouseId { get; }
    public decimal Quantity { get; }
}
