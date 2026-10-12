namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned PickWaveLine; tenant scope and input fields validated at creation.</summary>
public sealed class PickWaveLine : OrganizationEntity
{
    public PickWaveLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid pickWaveId,
        Guid pickingTaskId,
        Guid inventoryItemId,
        decimal requestedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        PickWaveId = DomainRules.Id(pickWaveId, nameof(pickWaveId));
        PickingTaskId = DomainRules.Id(pickingTaskId, nameof(pickingTaskId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        RequestedQuantity = DomainRules.Positive(requestedQuantity, nameof(requestedQuantity));
    }

    public Guid PickWaveId { get; }
    public Guid PickingTaskId { get; }
    public Guid InventoryItemId { get; }
    public decimal RequestedQuantity { get; }
}
