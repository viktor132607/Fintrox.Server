namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryCostAdjustment domain record; references checked in application layer.</summary>
public sealed class InventoryCostAdjustment : OrganizationEntity
{
    public InventoryCostAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid sourceMovementId,
        DateOnly adjustmentOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        SourceMovementId = DomainRules.Id(sourceMovementId, nameof(sourceMovementId));
        AdjustmentOn = DomainRules.Date(adjustmentOn, nameof(adjustmentOn));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid InventoryItemId { get; }
    public Guid SourceMovementId { get; }
    public DateOnly AdjustmentOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
