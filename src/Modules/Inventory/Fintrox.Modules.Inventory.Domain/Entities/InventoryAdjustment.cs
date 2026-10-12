namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryAdjustment domain record; references checked in application layer.</summary>
public sealed class InventoryAdjustment : OrganizationEntity
{
    public InventoryAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string adjustmentNumber,
        DateOnly adjustmentOn,
        string reason,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        AdjustmentNumber = DomainRules.Text(adjustmentNumber, nameof(adjustmentNumber));
        AdjustmentOn = DomainRules.Date(adjustmentOn, nameof(adjustmentOn));
        Reason = DomainRules.Text(reason, nameof(reason));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WarehouseId { get; }
    public string AdjustmentNumber { get; }
    public DateOnly AdjustmentOn { get; }
    public string Reason { get; }
    public string Status { get; }
}
