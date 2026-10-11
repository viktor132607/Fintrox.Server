namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

public sealed class CycleCountLine : OrganizationEntity
{
    public CycleCountLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid cycleCountId,
        Guid itemId,
        decimal countedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        CycleCountId = DomainRules.Id(cycleCountId, nameof(cycleCountId));
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        CountedQuantity = DomainRules.Nonnegative(countedQuantity, nameof(countedQuantity));
    }

    public Guid CycleCountId { get; }
    public Guid ItemId { get; }
    public decimal CountedQuantity { get; }
}
