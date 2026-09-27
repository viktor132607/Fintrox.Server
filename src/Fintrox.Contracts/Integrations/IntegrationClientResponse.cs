namespace Fintrox.Contracts.Integrations;

public sealed record IntegrationClientResponse(
    Guid Id,
    Guid OrganizationId,
    string ClientId,
    string Name,
    IReadOnlyList<string> Scopes,
    string SecretPrefix,
    bool IsActive,
    DateTimeOffset SecretRotatedAtUtc,
    DateTimeOffset? LastUsedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
