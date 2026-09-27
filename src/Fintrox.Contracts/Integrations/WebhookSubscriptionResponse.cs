namespace Fintrox.Contracts.Integrations;

public sealed record WebhookSubscriptionResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string TargetUrl,
    IReadOnlyList<string> EventTypes,
    string SecretPrefix,
    bool IsActive,
    DateTimeOffset SecretRotatedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
