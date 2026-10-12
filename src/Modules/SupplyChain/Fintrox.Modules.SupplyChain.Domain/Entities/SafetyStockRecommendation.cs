namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SafetyStockRecommendation domain record. No cross-module navigation properties.</summary>
public sealed class SafetyStockRecommendation : OrganizationEntity
{
    public SafetyStockRecommendation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid warehouseId,
        decimal recommendedQuantity,
        DateOnly calculatedOn)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        RecommendedQuantity = DomainRules.Nonnegative(recommendedQuantity, nameof(recommendedQuantity));
        CalculatedOn = DomainRules.Date(calculatedOn, nameof(calculatedOn));
    }

    public Guid InventoryItemId { get; }
    public Guid WarehouseId { get; }
    public decimal RecommendedQuantity { get; }
    public DateOnly CalculatedOn { get; }
}
