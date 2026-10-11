namespace Fintrox.Modules.SupplyChain.Domain.Entities;

public sealed class SupplierCommitment : OrganizationEntity
{
    public SupplierCommitment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        Guid purchaseOrderLineId,
        DateOnly promisedOn,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        PurchaseOrderLineId = DomainRules.Id(purchaseOrderLineId, nameof(purchaseOrderLineId));
        PromisedOn = DomainRules.Date(promisedOn, nameof(promisedOn));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid SupplierId { get; }
    public Guid PurchaseOrderLineId { get; }
    public DateOnly PromisedOn { get; }
    public decimal Quantity { get; }
}
