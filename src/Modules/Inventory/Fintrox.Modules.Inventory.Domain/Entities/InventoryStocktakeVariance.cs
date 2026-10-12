namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryStocktakeVariance domain record; references checked in application layer.</summary>
public sealed class InventoryStocktakeVariance : OrganizationEntity
{
    public InventoryStocktakeVariance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid stocktakeSessionId,
        Guid inventoryItemId,
        decimal varianceQuantity,
        decimal varianceValue,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        StocktakeSessionId = DomainRules.Id(stocktakeSessionId, nameof(stocktakeSessionId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        VarianceQuantity = DomainRules.Nonzero(varianceQuantity, nameof(varianceQuantity));
        VarianceValue = DomainRules.Nonzero(varianceValue, nameof(varianceValue));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid StocktakeSessionId { get; }
    public Guid InventoryItemId { get; }
    public decimal VarianceQuantity { get; }
    public decimal VarianceValue { get; }
    public string Currency { get; }
}
