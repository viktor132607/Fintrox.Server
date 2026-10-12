namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionPlanAddon domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionPlanAddon : OrganizationEntity
{
    public SubscriptionPlanAddon(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionPlanId,
        string addonCode,
        decimal addonPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionPlanId = DomainRules.Id(subscriptionPlanId, nameof(subscriptionPlanId));
        AddonCode = DomainRules.Text(addonCode, nameof(addonCode));
        AddonPrice = DomainRules.Nonnegative(addonPrice, nameof(addonPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SubscriptionPlanId { get; }
    public string AddonCode { get; }
    public decimal AddonPrice { get; }
    public string Currency { get; }
}
