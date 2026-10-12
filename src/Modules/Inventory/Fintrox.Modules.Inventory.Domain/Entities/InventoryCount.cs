namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryCount domain record; references checked in application layer.</summary>
public sealed class InventoryCount : OrganizationEntity
{
    public InventoryCount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string countReference,
        DateOnly countedOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        CountReference = DomainRules.Text(countReference, nameof(countReference));
        CountedOn = DomainRules.Date(countedOn, nameof(countedOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WarehouseId { get; }
    public string CountReference { get; }
    public DateOnly CountedOn { get; }
    public string Status { get; }
}
