namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionProration domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionProration : OrganizationEntity
{
    public SubscriptionProration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        DateOnly prorationOn,
        Guid oldPlanId,
        Guid newPlanId,
        decimal proratedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        ProrationOn = DomainRules.Date(prorationOn, nameof(prorationOn));
        OldPlanId = DomainRules.Id(oldPlanId, nameof(oldPlanId));
        NewPlanId = DomainRules.Id(newPlanId, nameof(newPlanId));
        ProratedAmount = DomainRules.Nonzero(proratedAmount, nameof(proratedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SubscriptionId { get; }
    public DateOnly ProrationOn { get; }
    public Guid OldPlanId { get; }
    public Guid NewPlanId { get; }
    public decimal ProratedAmount { get; }
    public string Currency { get; }
}
