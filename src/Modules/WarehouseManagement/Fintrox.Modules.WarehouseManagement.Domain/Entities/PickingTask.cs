namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

public sealed class PickingTask : OrganizationEntity
{
    public PickingTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        Guid binLocationId,
        Guid fulfillmentLineId,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        BinLocationId = DomainRules.Id(binLocationId, nameof(binLocationId));
        FulfillmentLineId = DomainRules.Id(fulfillmentLineId, nameof(fulfillmentLineId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid WarehouseId { get; }
    public Guid BinLocationId { get; }
    public Guid FulfillmentLineId { get; }
    public decimal Quantity { get; }
}
