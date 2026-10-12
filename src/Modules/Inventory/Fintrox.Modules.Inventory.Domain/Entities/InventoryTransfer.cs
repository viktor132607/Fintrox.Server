namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryTransfer domain record; references checked in application layer.</summary>
public sealed class InventoryTransfer : OrganizationEntity
{
    public InventoryTransfer(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        string reference,
        DateOnly requestedOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SourceWarehouseId = DomainRules.Id(sourceWarehouseId, nameof(sourceWarehouseId));
        DestinationWarehouseId = DomainRules.Id(destinationWarehouseId, nameof(destinationWarehouseId));
        Reference = DomainRules.Text(reference, nameof(reference));
        RequestedOn = DomainRules.Date(requestedOn, nameof(requestedOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SourceWarehouseId { get; }
    public Guid DestinationWarehouseId { get; }
    public string Reference { get; }
    public DateOnly RequestedOn { get; }
    public string Status { get; }
}
