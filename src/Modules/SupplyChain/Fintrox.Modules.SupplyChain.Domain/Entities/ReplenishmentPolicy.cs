namespace Fintrox.Modules.SupplyChain.Domain.Entities;

public sealed class ReplenishmentPolicy : OrganizationEntity
{
    public ReplenishmentPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        Guid warehouseId,
        decimal reorderPoint,
        decimal targetQuantity,
        int leadTimeDays)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        ReorderPoint = DomainRules.Nonnegative(reorderPoint, nameof(reorderPoint));
        TargetQuantity = DomainRules.Positive(targetQuantity, nameof(targetQuantity));
        LeadTimeDays = DomainRules.NonnegativeInt(leadTimeDays, nameof(leadTimeDays));
        if (TargetQuantity < ReorderPoint) throw new ArgumentException("Target quantity cannot be below the reorder point.", nameof(targetQuantity));
    }

    public Guid ItemId { get; }
    public Guid WarehouseId { get; }
    public decimal ReorderPoint { get; }
    public decimal TargetQuantity { get; }
    public int LeadTimeDays { get; }
}
