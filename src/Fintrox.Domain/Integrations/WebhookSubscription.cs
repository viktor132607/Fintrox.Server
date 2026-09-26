using Fintrox.Domain.Common;

namespace Fintrox.Domain.Integrations;

public sealed class WebhookSubscription
    : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private WebhookSubscription()
    {
    }

    private WebhookSubscription(
        Guid id,
        Guid organizationId,
        string name,
        string targetUrl,
        string eventTypes,
        string signingSecretCiphertext,
        string secretPrefix,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        Name = NormalizeRequired(
            name,
            nameof(name),
            160);
        TargetUrl = NormalizeUrl(targetUrl);
        EventTypes = NormalizeRequired(
            eventTypes,
            nameof(eventTypes),
            2000);
        SigningSecretCiphertext = NormalizeRequired(
            signingSecretCiphertext,
            nameof(signingSecretCiphertext),
            4096);
        SecretPrefix = NormalizeRequired(
            secretPrefix,
            nameof(secretPrefix),
            24);
        IsActive = true;
        SecretRotatedAtUtc = now;
    }

    public string Name { get; private set; } = null!;

    public string TargetUrl { get; private set; } = null!;

    public string EventTypes { get; private set; } = null!;

    public string SigningSecretCiphertext { get; private set; } = null!;

    public string SecretPrefix { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTimeOffset SecretRotatedAtUtc { get; private set; }

    public static WebhookSubscription Create(
        Guid organizationId,
        string name,
        string targetUrl,
        string eventTypes,
        string signingSecretCiphertext,
        string secretPrefix,
        DateTimeOffset now) =>
        new(
            Guid.NewGuid(),
            organizationId,
            name,
            targetUrl,
            eventTypes,
            signingSecretCiphertext,
            secretPrefix,
            now);

    public void Update(
        string name,
        string targetUrl,
        string eventTypes,
        DateTimeOffset now)
    {
        Name = NormalizeRequired(
            name,
            nameof(name),
            160);
        TargetUrl = NormalizeUrl(targetUrl);
        EventTypes = NormalizeRequired(
            eventTypes,
            nameof(eventTypes),
            2000);
        Touch(now);
    }

    public void RotateSecret(
        string signingSecretCiphertext,
        string secretPrefix,
        DateTimeOffset now)
    {
        SigningSecretCiphertext = NormalizeRequired(
            signingSecretCiphertext,
            nameof(signingSecretCiphertext),
            4096);
        SecretPrefix = NormalizeRequired(
            secretPrefix,
            nameof(secretPrefix),
            24);
        SecretRotatedAtUtc = now;
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

    public void Activate(DateTimeOffset now)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(now);
    }

    public bool Matches(string eventType)
    {
        var normalized = eventType.Trim().ToLowerInvariant();

        return EventTypes
            .Split(',', StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries)
            .Any(item => item == "*" ||
                         string.Equals(
                             item,
                             normalized,
                             StringComparison.Ordinal));
    }

    private static string NormalizeUrl(string value)
    {
        var normalized = NormalizeRequired(
            value,
            nameof(value),
            2048);

        if (!Uri.TryCreate(
                normalized,
                UriKind.Absolute,
                out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps &&
             uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new ArgumentException(
                "Webhook target URL must be an absolute HTTP or HTTPS URL.",
                nameof(value));
        }

        return uri.AbsoluteUri;
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
