namespace Fintrox.Modules.SupplyChain.Domain.Entities;

public sealed class ProcurementPlan : OrganizationEntity
{
    public ProcurementPlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        Guid itemId,
        DateOnly requiredOn,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        RequiredOn = DomainRules.Date(requiredOn, nameof(requiredOn));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid SupplierId { get; }
    public Guid ItemId { get; }
    public DateOnly RequiredOn { get; }
    public decimal Quantity { get; }
}
