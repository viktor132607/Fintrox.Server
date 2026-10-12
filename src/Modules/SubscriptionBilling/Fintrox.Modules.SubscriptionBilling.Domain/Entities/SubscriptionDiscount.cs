namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionDiscount domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionDiscount : OrganizationEntity
{
    public SubscriptionDiscount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        string discountCode,
        decimal discountPercent,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        DiscountCode = DomainRules.Text(discountCode, nameof(discountCode));
        DiscountPercent = DomainRules.Percent(discountPercent, nameof(discountPercent));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid SubscriptionId { get; }
    public string DiscountCode { get; }
    public decimal DiscountPercent { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
