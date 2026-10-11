namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

public sealed class ReceivingTask : OrganizationEntity
{
    public ReceivingTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        Guid purchaseOrderLineId,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        PurchaseOrderLineId = DomainRules.Id(purchaseOrderLineId, nameof(purchaseOrderLineId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid WarehouseId { get; }
    public Guid PurchaseOrderLineId { get; }
    public decimal Quantity { get; }
}
