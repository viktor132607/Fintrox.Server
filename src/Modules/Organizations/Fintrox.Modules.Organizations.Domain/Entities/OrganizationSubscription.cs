namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationSubscription record.</summary>
public sealed class OrganizationSubscription : OrganizationEntity
{
    public OrganizationSubscription(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string subscriptionPlanCode,
        DateOnly startsOn,
        DateOnly endsOn,
        int seatLimit)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionPlanCode = DomainRules.Text(subscriptionPlanCode, nameof(subscriptionPlanCode));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        SeatLimit = DomainRules.NonnegativeInt(seatLimit, nameof(seatLimit));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public string SubscriptionPlanCode { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public int SeatLimit { get; }
}
