namespace Fintrox.Application.Integrations;

public sealed record IntegrationInboxCommand(
    string SourceSystem,
    string ExternalId,
    string EventType,
    string Operation,
    string PayloadJson);
