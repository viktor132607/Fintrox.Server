namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned WebhookDeliveryAttempt domain record; references checked in application layer.</summary>
public sealed class WebhookDeliveryAttempt : OrganizationEntity
{
    public WebhookDeliveryAttempt(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionConfigId,
        Guid eventReferenceId,
        int attemptNumber,
        DateTimeOffset attemptAtUtc,
        int statusCode)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionConfigId = DomainRules.Id(subscriptionConfigId, nameof(subscriptionConfigId));
        EventReferenceId = DomainRules.Id(eventReferenceId, nameof(eventReferenceId));
        AttemptNumber = DomainRules.PositiveInt(attemptNumber, nameof(attemptNumber));
        AttemptAtUtc = DomainRules.Instant(attemptAtUtc, nameof(attemptAtUtc));
        StatusCode = DomainRules.NonnegativeInt(statusCode, nameof(statusCode));
    }

    public Guid SubscriptionConfigId { get; }
    public Guid EventReferenceId { get; }
    public int AttemptNumber { get; }
    public DateTimeOffset AttemptAtUtc { get; }
    public int StatusCode { get; }
}
