namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned PromotionRedemption; external references validated by application layer.</summary>
public sealed class PromotionRedemption : OrganizationEntity
{
    public PromotionRedemption(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid promotionId,
        Guid couponId,
        Guid commerceOrderId,
        DateTimeOffset redeemedAtUtc,
        decimal discountAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PromotionId = DomainRules.Id(promotionId, nameof(promotionId));
        CouponId = DomainRules.Id(couponId, nameof(couponId));
        CommerceOrderId = DomainRules.Id(commerceOrderId, nameof(commerceOrderId));
        RedeemedAtUtc = DomainRules.Instant(redeemedAtUtc, nameof(redeemedAtUtc));
        DiscountAmount = DomainRules.Nonnegative(discountAmount, nameof(discountAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PromotionId { get; }
    public Guid CouponId { get; }
    public Guid CommerceOrderId { get; }
    public DateTimeOffset RedeemedAtUtc { get; }
    public decimal DiscountAmount { get; }
    public string Currency { get; }
}
