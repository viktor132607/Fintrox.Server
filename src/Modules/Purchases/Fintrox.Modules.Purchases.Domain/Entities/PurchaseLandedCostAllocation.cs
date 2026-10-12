namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseLandedCostAllocation model. References require application validation.</summary>
public sealed class PurchaseLandedCostAllocation : OrganizationEntity
{
    public PurchaseLandedCostAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid landedCostId,
        Guid goodsReceiptLineId,
        decimal allocatedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        LandedCostId = DomainRules.Id(landedCostId, nameof(landedCostId));
        GoodsReceiptLineId = DomainRules.Id(goodsReceiptLineId, nameof(goodsReceiptLineId));
        AllocatedAmount = DomainRules.Nonnegative(allocatedAmount, nameof(allocatedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid LandedCostId { get; }
    public Guid GoodsReceiptLineId { get; }
    public decimal AllocatedAmount { get; }
    public string Currency { get; }
}
