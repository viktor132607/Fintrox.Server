namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionTrial domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionTrial : OrganizationEntity
{
    public SubscriptionTrial(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        DateOnly trialStartsOn,
        DateOnly trialEndsOn,
        bool isConverted)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        TrialStartsOn = DomainRules.Date(trialStartsOn, nameof(trialStartsOn));
        TrialEndsOn = DomainRules.Date(trialEndsOn, nameof(trialEndsOn));
        IsConverted = isConverted;
    }

    public Guid SubscriptionId { get; }
    public DateOnly TrialStartsOn { get; }
    public DateOnly TrialEndsOn { get; }
    public bool IsConverted { get; }
}
