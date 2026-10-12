namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventorySerialHistory domain record; references checked in application layer.</summary>
public sealed class InventorySerialHistory : OrganizationEntity
{
    public InventorySerialHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventorySerialId,
        Guid movementId,
        DateTimeOffset occurredAtUtc,
        string eventCode)
        : base(id, organizationId, createdAtUtc)
    {
        InventorySerialId = DomainRules.Id(inventorySerialId, nameof(inventorySerialId));
        MovementId = DomainRules.Id(movementId, nameof(movementId));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        EventCode = DomainRules.Text(eventCode, nameof(eventCode));
    }

    public Guid InventorySerialId { get; }
    public Guid MovementId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string EventCode { get; }
}
