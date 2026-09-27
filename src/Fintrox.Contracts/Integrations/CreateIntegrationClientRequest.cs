namespace Fintrox.Contracts.Integrations;

public sealed record CreateIntegrationClientRequest(
    string Name,
    IReadOnlyCollection<string> Scopes);
