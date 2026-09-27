namespace Fintrox.Contracts.Integrations;

public sealed record IntegrationClientSecretResponse(
    IntegrationClientResponse Client,
    string ClientSecret);
