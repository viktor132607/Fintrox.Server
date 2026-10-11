namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

public sealed class Subscription : OrganizationEntity
{
    public Subscription(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid planId,
        Guid counterpartyId,
        DateOnly startsOn,
        DateOnly endsOn,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        PlanId = DomainRules.Id(planId, nameof(planId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid PlanId { get; }
    public Guid CounterpartyId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public decimal Quantity { get; }
}
