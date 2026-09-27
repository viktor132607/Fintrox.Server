namespace Fintrox.Contracts.Integrations;

public sealed record IntegrationFailureResponse(
    Guid Id,
    Guid OrganizationId,
    string Kind,
    Guid ReferenceId,
    string Reason,
    string PayloadJson,
    int RetryCount,
    DateTimeOffset LastAttemptAtUtc,
    bool IsResolved,
    DateTimeOffset? ResolvedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
