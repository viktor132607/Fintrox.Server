namespace Fintrox.Modules.Inventory.Domain.Entities;

public sealed class StockMovement : OrganizationEntity
{
    public StockMovement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        Guid warehouseId,
        StockMovementKind kind,
        decimal quantity,
        DateTimeOffset occurredAtUtc,
        Guid sourceDocumentId)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        Kind = DomainRules.Defined(kind, nameof(kind));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
    }

    public Guid ItemId { get; }
    public Guid WarehouseId { get; }
    public StockMovementKind Kind { get; }
    public decimal Quantity { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public Guid SourceDocumentId { get; }
}
