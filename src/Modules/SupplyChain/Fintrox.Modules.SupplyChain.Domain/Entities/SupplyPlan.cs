namespace Fintrox.Modules.SupplyChain.Domain.Entities;

public sealed class SupplyPlan : OrganizationEntity
{
    public SupplyPlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        Guid warehouseId,
        DateOnly requiredOn,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        RequiredOn = DomainRules.Date(requiredOn, nameof(requiredOn));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid ItemId { get; }
    public Guid WarehouseId { get; }
    public DateOnly RequiredOn { get; }
    public decimal Quantity { get; }
}
