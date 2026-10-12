namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned PickingConfirmation; tenant scope and input fields validated at creation.</summary>
public sealed class PickingConfirmation : OrganizationEntity
{
    public PickingConfirmation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid pickingTaskId,
        Guid binLocationId,
        Guid inventoryItemId,
        decimal pickedQuantity,
        DateTimeOffset confirmedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PickingTaskId = DomainRules.Id(pickingTaskId, nameof(pickingTaskId));
        BinLocationId = DomainRules.Id(binLocationId, nameof(binLocationId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        PickedQuantity = DomainRules.Positive(pickedQuantity, nameof(pickedQuantity));
        ConfirmedAtUtc = DomainRules.Instant(confirmedAtUtc, nameof(confirmedAtUtc));
    }

    public Guid PickingTaskId { get; }
    public Guid BinLocationId { get; }
    public Guid InventoryItemId { get; }
    public decimal PickedQuantity { get; }
    public DateTimeOffset ConfirmedAtUtc { get; }
}
