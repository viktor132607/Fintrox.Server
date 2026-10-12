namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionPlanPrice domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionPlanPrice : OrganizationEntity
{
    public SubscriptionPlanPrice(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionPlanId,
        string currency,
        decimal amount,
        int billingIntervalMonths)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionPlanId = DomainRules.Id(subscriptionPlanId, nameof(subscriptionPlanId));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        BillingIntervalMonths = DomainRules.PositiveInt(billingIntervalMonths, nameof(billingIntervalMonths));
    }

    public Guid SubscriptionPlanId { get; }
    public string Currency { get; }
    public decimal Amount { get; }
    public int BillingIntervalMonths { get; }
}
