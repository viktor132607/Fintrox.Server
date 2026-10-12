namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionPlanVersion domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionPlanVersion : OrganizationEntity
{
    public SubscriptionPlanVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionPlanId,
        int versionNumber,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionPlanId = DomainRules.Id(subscriptionPlanId, nameof(subscriptionPlanId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public Guid SubscriptionPlanId { get; }
    public int VersionNumber { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
