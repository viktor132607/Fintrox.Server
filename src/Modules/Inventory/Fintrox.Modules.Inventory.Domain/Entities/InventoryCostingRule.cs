namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryCostingRule domain record; references checked in application layer.</summary>
public sealed class InventoryCostingRule : OrganizationEntity
{
    public InventoryCostingRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        string costingMethod,
        DateOnly effectiveFrom,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        CostingMethod = DomainRules.Text(costingMethod, nameof(costingMethod));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid InventoryItemId { get; }
    public string CostingMethod { get; }
    public DateOnly EffectiveFrom { get; }
    public string Currency { get; }
}
