namespace Fintrox.Modules.Purchases.Domain.Entities;

public sealed class GoodsReceipt : OrganizationEntity
{
    public GoodsReceipt(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseOrderId,
        Guid warehouseId,
        string reference,
        DateOnly receivedOn)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseOrderId = DomainRules.Id(purchaseOrderId, nameof(purchaseOrderId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        Reference = DomainRules.Text(reference, nameof(reference));
        ReceivedOn = DomainRules.Date(receivedOn, nameof(receivedOn));
    }

    public Guid PurchaseOrderId { get; }
    public Guid WarehouseId { get; }
    public string Reference { get; }
    public DateOnly ReceivedOn { get; }
}
