namespace Fintrox.Modules.SupplyChain.Domain.Entities;

public sealed class DemandForecast : OrganizationEntity
{
    public DemandForecast(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        Guid warehouseId,
        DateOnly startsOn,
        DateOnly endsOn,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Quantity = DomainRules.Nonnegative(quantity, nameof(quantity));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid ItemId { get; }
    public Guid WarehouseId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public decimal Quantity { get; }
}
