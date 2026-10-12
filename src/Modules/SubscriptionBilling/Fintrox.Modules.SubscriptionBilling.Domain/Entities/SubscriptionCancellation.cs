namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionCancellation domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionCancellation : OrganizationEntity
{
    public SubscriptionCancellation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        DateOnly requestedOn,
        DateOnly effectiveOn,
        string reason,
        bool isImmediate)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        RequestedOn = DomainRules.Date(requestedOn, nameof(requestedOn));
        EffectiveOn = DomainRules.Date(effectiveOn, nameof(effectiveOn));
        Reason = DomainRules.Text(reason, nameof(reason));
        IsImmediate = isImmediate;
    }

    public Guid SubscriptionId { get; }
    public DateOnly RequestedOn { get; }
    public DateOnly EffectiveOn { get; }
    public string Reason { get; }
    public bool IsImmediate { get; }
}
