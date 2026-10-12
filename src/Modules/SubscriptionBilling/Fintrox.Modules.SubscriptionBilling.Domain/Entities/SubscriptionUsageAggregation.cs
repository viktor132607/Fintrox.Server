namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionUsageAggregation domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionUsageAggregation : OrganizationEntity
{
    public SubscriptionUsageAggregation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        Guid usageMeterId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal totalUnits)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        UsageMeterId = DomainRules.Id(usageMeterId, nameof(usageMeterId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        TotalUnits = DomainRules.Nonnegative(totalUnits, nameof(totalUnits));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid SubscriptionId { get; }
    public Guid UsageMeterId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal TotalUnits { get; }
}
