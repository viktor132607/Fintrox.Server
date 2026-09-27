namespace Fintrox.Contracts.Integrations;

public sealed record IntegrationInboxResponse(
    Guid Id,
    Guid OrganizationId,
    string SourceSystem,
    string ExternalId,
    string EventType,
    string Operation,
    string PayloadJson,
    string Status,
    int RetryCount,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? ResultEntityType,
    Guid? ResultEntityId,
    Guid? JournalEntryId,
    string? FailureReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
