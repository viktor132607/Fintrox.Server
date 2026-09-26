namespace Fintrox.Contracts.Integrations;

public sealed record IntegrationRequestResponse(
    Guid Id,
    Guid OrganizationId,
    Guid IntegrationClientId,
    string SourceSystem,
    string ExternalId,
    string EventType,
    string RequestMethod,
    string RequestPath,
    string RequestHash,
    string Status,
    int? ResponseStatusCode,
    string? ResourceReference,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);
