namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionChangeHistory domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionChangeHistory : OrganizationEntity
{
    public SubscriptionChangeHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        Guid previousPlanId,
        Guid newPlanId,
        DateTimeOffset changedAtUtc,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        PreviousPlanId = DomainRules.Id(previousPlanId, nameof(previousPlanId));
        NewPlanId = DomainRules.Id(newPlanId, nameof(newPlanId));
        ChangedAtUtc = DomainRules.Instant(changedAtUtc, nameof(changedAtUtc));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid SubscriptionId { get; }
    public Guid PreviousPlanId { get; }
    public Guid NewPlanId { get; }
    public DateTimeOffset ChangedAtUtc { get; }
    public string Reason { get; }
}
