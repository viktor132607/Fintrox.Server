namespace Fintrox.Contracts.Integrations;

public sealed record UpdateIntegrationClientRequest(
    string Name,
    IReadOnlyCollection<string> Scopes);
