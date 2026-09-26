namespace Fintrox.Contracts.Integrations;

public sealed record IntegrationTokenRequest(
    string GrantType,
    string ClientId,
    string ClientSecret);
