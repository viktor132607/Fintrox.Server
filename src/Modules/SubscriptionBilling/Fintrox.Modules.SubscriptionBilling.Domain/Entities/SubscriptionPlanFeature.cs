namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionPlanFeature domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionPlanFeature : OrganizationEntity
{
    public SubscriptionPlanFeature(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionPlanId,
        string featureCode,
        decimal includedUnits,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionPlanId = DomainRules.Id(subscriptionPlanId, nameof(subscriptionPlanId));
        FeatureCode = DomainRules.Text(featureCode, nameof(featureCode));
        IncludedUnits = DomainRules.Nonnegative(includedUnits, nameof(includedUnits));
        IsEnabled = isEnabled;
    }

    public Guid SubscriptionPlanId { get; }
    public string FeatureCode { get; }
    public decimal IncludedUnits { get; }
    public bool IsEnabled { get; }
}
