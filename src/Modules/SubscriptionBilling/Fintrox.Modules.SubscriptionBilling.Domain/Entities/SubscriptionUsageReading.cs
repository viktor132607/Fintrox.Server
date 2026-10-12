namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionUsageReading domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionUsageReading : OrganizationEntity
{
    public SubscriptionUsageReading(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        Guid usageMeterId,
        DateOnly measuredOn,
        decimal consumedUnits)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        UsageMeterId = DomainRules.Id(usageMeterId, nameof(usageMeterId));
        MeasuredOn = DomainRules.Date(measuredOn, nameof(measuredOn));
        ConsumedUnits = DomainRules.Nonnegative(consumedUnits, nameof(consumedUnits));
    }

    public Guid SubscriptionId { get; }
    public Guid UsageMeterId { get; }
    public DateOnly MeasuredOn { get; }
    public decimal ConsumedUnits { get; }
}
