namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionPause domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionPause : OrganizationEntity
{
    public SubscriptionPause(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        DateOnly startsOn,
        DateOnly endsOn,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Reason = DomainRules.Text(reason, nameof(reason));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid SubscriptionId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string Reason { get; }
}
