using Fintrox.Domain.Common;

namespace Fintrox.Domain.Integrations;

public sealed class IntegrationClient
    : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private IntegrationClient()
    {
    }

    private IntegrationClient(
        Guid id,
        Guid organizationId,
        string clientId,
        string name,
        string secretHash,
        string secretPrefix,
        string scopes,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        ClientId = NormalizeRequired(
            clientId,
            nameof(clientId),
            80);
        Name = NormalizeRequired(
            name,
            nameof(name),
            160);
        SecretHash = NormalizeRequired(
            secretHash,
            nameof(secretHash),
            128);
        SecretPrefix = NormalizeRequired(
            secretPrefix,
            nameof(secretPrefix),
            24);
        Scopes = NormalizeRequired(
            scopes,
            nameof(scopes),
            2000);
        SecretRotatedAtUtc = now;
        IsActive = true;
    }

    public string ClientId { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string SecretHash { get; private set; } = null!;

    public string SecretPrefix { get; private set; } = null!;

    public string Scopes { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTimeOffset SecretRotatedAtUtc { get; private set; }

    public DateTimeOffset? LastUsedAtUtc { get; private set; }

    public static IntegrationClient Create(
        Guid organizationId,
        string clientId,
        string name,
        string secretHash,
        string secretPrefix,
        string scopes,
        DateTimeOffset now) =>
        new(
            Guid.NewGuid(),
            organizationId,
            clientId,
            name,
            secretHash,
            secretPrefix,
            scopes,
            now);

    public void Update(
        string name,
        string scopes,
        DateTimeOffset now)
    {
        Name = NormalizeRequired(
            name,
            nameof(name),
            160);
        Scopes = NormalizeRequired(
            scopes,
            nameof(scopes),
            2000);
        Touch(now);
    }

    public void RotateSecret(
        string secretHash,
        string secretPrefix,
        DateTimeOffset now)
    {
        SecretHash = NormalizeRequired(
            secretHash,
            nameof(secretHash),
            128);
        SecretPrefix = NormalizeRequired(
            secretPrefix,
            nameof(secretPrefix),
            24);
        SecretRotatedAtUtc = now;
        Touch(now);
    }

    public void MarkUsed(DateTimeOffset now)
    {
        LastUsedAtUtc = now;
        Touch(now);
    }

    public void Activate(DateTimeOffset now)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(now);
    }

    public void Deactivate(DateTimeOffset now)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(now);
    }

    private static string NormalizeRequired(
        string value,
        string parameterName,
        int maxLength)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "Value is required.",
                parameterName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }
}
