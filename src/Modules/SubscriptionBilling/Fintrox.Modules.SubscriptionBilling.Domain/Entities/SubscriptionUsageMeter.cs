namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionUsageMeter domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionUsageMeter : OrganizationEntity
{
    public SubscriptionUsageMeter(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionPlanId,
        string meterCode,
        string unitName,
        string aggregationMethod)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionPlanId = DomainRules.Id(subscriptionPlanId, nameof(subscriptionPlanId));
        MeterCode = DomainRules.Text(meterCode, nameof(meterCode));
        UnitName = DomainRules.Text(unitName, nameof(unitName));
        AggregationMethod = DomainRules.Text(aggregationMethod, nameof(aggregationMethod));
    }

    public Guid SubscriptionPlanId { get; }
    public string MeterCode { get; }
    public string UnitName { get; }
    public string AggregationMethod { get; }
}
