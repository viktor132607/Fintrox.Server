namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseLandedCost model. References require application validation.</summary>
public sealed class PurchaseLandedCost : OrganizationEntity
{
    public PurchaseLandedCost(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid goodsReceiptId,
        string costType,
        decimal amount,
        string currency,
        Guid documentId)
        : base(id, organizationId, createdAtUtc)
    {
        GoodsReceiptId = DomainRules.Id(goodsReceiptId, nameof(goodsReceiptId));
        CostType = DomainRules.Text(costType, nameof(costType));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
    }

    public Guid GoodsReceiptId { get; }
    public string CostType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public Guid DocumentId { get; }
}
