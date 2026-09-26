using Fintrox.Domain.Common;

namespace Fintrox.Domain.Integrations;

public sealed class WebhookDelivery
    : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private WebhookDelivery()
    {
    }

    private WebhookDelivery(
        Guid id,
        Guid organizationId,
        Guid integrationEventId,
        Guid webhookSubscriptionId,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        if (integrationEventId == Guid.Empty)
        {
            throw new ArgumentException(
                "Integration event id is required.",
                nameof(integrationEventId));
        }

        if (webhookSubscriptionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Webhook subscription id is required.",
                nameof(webhookSubscriptionId));
        }

        IntegrationEventId = integrationEventId;
        WebhookSubscriptionId = webhookSubscriptionId;
        Status = WebhookDeliveryStatus.Pending;
        NextAttemptAtUtc = now;
    }

    public Guid IntegrationEventId { get; private set; }

    public Guid WebhookSubscriptionId { get; private set; }

    public WebhookDeliveryStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset NextAttemptAtUtc { get; private set; }

    public DateTimeOffset? LastAttemptAtUtc { get; private set; }

    public int? LastStatusCode { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset? DeliveredAtUtc { get; private set; }

    public static WebhookDelivery Create(
        Guid organizationId,
        Guid integrationEventId,
        Guid webhookSubscriptionId,
        DateTimeOffset now) =>
        new(
            Guid.NewGuid(),
            organizationId,
            integrationEventId,
            webhookSubscriptionId,
            now);

    public void StartAttempt(DateTimeOffset now)
    {
        if (Status != WebhookDeliveryStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending webhook deliveries can be attempted.");
        }

        AttemptCount++;
        LastAttemptAtUtc = now;
        Touch(now);
    }

    public void Succeed(
        int statusCode,
        DateTimeOffset now)
    {
        Status = WebhookDeliveryStatus.Delivered;
        LastStatusCode = statusCode;
        LastError = null;
        DeliveredAtUtc = now;
        Touch(now);
    }

    public void ScheduleRetry(
        int? statusCode,
        string error,
        DateTimeOffset nextAttemptAtUtc,
        DateTimeOffset now)
    {
        LastStatusCode = statusCode;
        LastError = NormalizeError(error);
        NextAttemptAtUtc = nextAttemptAtUtc;
        Status = WebhookDeliveryStatus.Pending;
        Touch(now);
    }

    public void Fail(
        int? statusCode,
        string error,
        DateTimeOffset now)
    {
        LastStatusCode = statusCode;
        LastError = NormalizeError(error);
        Status = WebhookDeliveryStatus.Failed;
        Touch(now);
    }

    public void Requeue(DateTimeOffset now)
    {
        Status = WebhookDeliveryStatus.Pending;
        NextAttemptAtUtc = now;
        LastError = null;
        LastStatusCode = null;
        Touch(now);
    }

    private static string NormalizeError(string value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "Webhook delivery failed.";
        }

        return normalized.Length <= 4000
            ? normalized
            : normalized[..4000];
    }
}
