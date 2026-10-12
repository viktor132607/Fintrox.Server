namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned PromotionCoupon; external references validated by application layer.</summary>
public sealed class PromotionCoupon : OrganizationEntity
{
    public PromotionCoupon(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid promotionId,
        string couponCode,
        int maxRedemptions,
        DateOnly expiresOn,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        PromotionId = DomainRules.Id(promotionId, nameof(promotionId));
        CouponCode = DomainRules.Text(couponCode, nameof(couponCode));
        MaxRedemptions = DomainRules.NonnegativeInt(maxRedemptions, nameof(maxRedemptions));
        ExpiresOn = DomainRules.Date(expiresOn, nameof(expiresOn));
        IsActive = isActive;
    }

    public Guid PromotionId { get; }
    public string CouponCode { get; }
    public int MaxRedemptions { get; }
    public DateOnly ExpiresOn { get; }
    public bool IsActive { get; }
}
