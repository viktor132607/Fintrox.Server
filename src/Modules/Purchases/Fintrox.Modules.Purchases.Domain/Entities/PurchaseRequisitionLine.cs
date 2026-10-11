namespace Fintrox.Modules.Purchases.Domain.Entities;

public sealed class PurchaseRequisitionLine : OrganizationEntity
{
    public PurchaseRequisitionLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid requisitionId,
        Guid itemId,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        RequisitionId = DomainRules.Id(requisitionId, nameof(requisitionId));
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid RequisitionId { get; }
    public Guid ItemId { get; }
    public decimal Quantity { get; }
}
