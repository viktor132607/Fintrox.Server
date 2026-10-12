namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionAddonEnrollment domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionAddonEnrollment : OrganizationEntity
{
    public SubscriptionAddonEnrollment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        string addonCode,
        decimal quantity,
        DateOnly startsOn)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        AddonCode = DomainRules.Text(addonCode, nameof(addonCode));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
    }

    public Guid SubscriptionId { get; }
    public string AddonCode { get; }
    public decimal Quantity { get; }
    public DateOnly StartsOn { get; }
}
