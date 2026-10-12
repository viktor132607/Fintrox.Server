namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseCycleCountVariance; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseCycleCountVariance : OrganizationEntity
{
    public WarehouseCycleCountVariance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid cycleCountLineId,
        Guid inventoryItemId,
        decimal expectedQuantity,
        decimal actualQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        CycleCountLineId = DomainRules.Id(cycleCountLineId, nameof(cycleCountLineId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        ExpectedQuantity = DomainRules.Nonnegative(expectedQuantity, nameof(expectedQuantity));
        ActualQuantity = DomainRules.Nonnegative(actualQuantity, nameof(actualQuantity));
    }

    public Guid CycleCountLineId { get; }
    public Guid InventoryItemId { get; }
    public decimal ExpectedQuantity { get; }
    public decimal ActualQuantity { get; }
}
