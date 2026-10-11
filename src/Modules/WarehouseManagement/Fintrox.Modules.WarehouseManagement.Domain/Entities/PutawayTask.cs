namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

public sealed class PutawayTask : OrganizationEntity
{
    public PutawayTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        Guid binLocationId,
        Guid itemId,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        BinLocationId = DomainRules.Id(binLocationId, nameof(binLocationId));
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid WarehouseId { get; }
    public Guid BinLocationId { get; }
    public Guid ItemId { get; }
    public decimal Quantity { get; }
}
