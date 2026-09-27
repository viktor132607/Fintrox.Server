namespace Fintrox.Contracts.Integrations;

public sealed record IntegrationTokenResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    long ExpiresInSeconds,
    string Scope);
