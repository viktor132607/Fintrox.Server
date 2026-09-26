namespace Fintrox.Application.Integrations;

public sealed record IntegrationIdempotencyRequest(
    Guid OrganizationId,
    Guid IntegrationClientId,
    string SourceSystem,
    string ExternalId,
    string EventType,
    string RequestMethod,
    string RequestPath,
    string RequestHash);
