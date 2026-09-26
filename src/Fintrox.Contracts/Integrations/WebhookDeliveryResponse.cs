namespace Fintrox.Contracts.Integrations;

public sealed record WebhookDeliveryResponse(
    Guid Id,
    Guid OrganizationId,
    Guid IntegrationEventId,
    Guid WebhookSubscriptionId,
    string Status,
    int AttemptCount,
    DateTimeOffset NextAttemptAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    int? LastStatusCode,
    string? LastError,
    DateTimeOffset? DeliveredAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
