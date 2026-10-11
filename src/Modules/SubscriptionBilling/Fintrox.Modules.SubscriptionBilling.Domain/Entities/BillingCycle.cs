namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

public sealed class BillingCycle : OrganizationEntity
{
    public BillingCycle(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        DateOnly startsOn,
        DateOnly endsOn,
        DateOnly dueOn)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid SubscriptionId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public DateOnly DueOn { get; }
}
