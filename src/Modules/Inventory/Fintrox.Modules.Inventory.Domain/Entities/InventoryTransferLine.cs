namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryTransferLine domain record; references checked in application layer.</summary>
public sealed class InventoryTransferLine : OrganizationEntity
{
    public InventoryTransferLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryTransferId,
        Guid inventoryItemId,
        decimal quantity,
        string unitCode,
        int lineNumber)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryTransferId = DomainRules.Id(inventoryTransferId, nameof(inventoryTransferId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        UnitCode = DomainRules.Text(unitCode, nameof(unitCode));
        LineNumber = DomainRules.PositiveInt(lineNumber, nameof(lineNumber));
    }

    public Guid InventoryTransferId { get; }
    public Guid InventoryItemId { get; }
    public decimal Quantity { get; }
    public string UnitCode { get; }
    public int LineNumber { get; }
}
