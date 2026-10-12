namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryStocktakeSession domain record; references checked in application layer.</summary>
public sealed class InventoryStocktakeSession : OrganizationEntity
{
    public InventoryStocktakeSession(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        DateTimeOffset startedAtUtc,
        Guid startedByUserId,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        StartedByUserId = DomainRules.Id(startedByUserId, nameof(startedByUserId));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WarehouseId { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public Guid StartedByUserId { get; }
    public string Status { get; }
}
