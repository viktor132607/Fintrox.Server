using Fintrox.Domain.Common;

namespace Fintrox.Domain.Integrations;

public sealed class IntegrationRequestRecord : Entity, IOrganizationScopedEntity
{
    private IntegrationRequestRecord()
    {
    }

    private IntegrationRequestRecord(
        Guid id,
        Guid organizationId,
        Guid integrationClientId,
        string sourceSystem,
        string externalId,
        string eventType,
        string requestMethod,
        string requestPath,
        string requestHash,
        DateTimeOffset now) : base(id)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Organization id is required.",
                nameof(organizationId));
        }

        if (integrationClientId == Guid.Empty)
        {
            throw new ArgumentException(
                "Integration client id is required.",
                nameof(integrationClientId));
        }

        OrganizationId = organizationId;
        IntegrationClientId = integrationClientId;
        SourceSystem = NormalizeKeyPart(
            sourceSystem,
            nameof(sourceSystem),
            100,
            toLower: true);
        ExternalId = NormalizeKeyPart(
            externalId,
            nameof(externalId),
            200,
            toLower: false);
        EventType = NormalizeKeyPart(
            eventType,
            nameof(eventType),
            100,
            toLower: true);
        RequestMethod = NormalizeKeyPart(
            requestMethod,
            nameof(requestMethod),
            16,
            toLower: false).ToUpperInvariant();
        RequestPath = NormalizeKeyPart(
            requestPath,
            nameof(requestPath),
            2048,
            toLower: false);
        RequestHash = NormalizeHash(requestHash);
        Status = IntegrationRequestStatus.Processing;
        CreatedAtUtc = now;
    }

    public Guid OrganizationId { get; private set; }

    public Guid IntegrationClientId { get; private set; }

    public string SourceSystem { get; private set; } = null!;

    public string ExternalId { get; private set; } = null!;

    public string EventType { get; private set; } = null!;

    public string RequestMethod { get; private set; } = null!;

    public string RequestPath { get; private set; } = null!;

    public string RequestHash { get; private set; } = null!;

    public IntegrationRequestStatus Status { get; private set; }

    public int? ResponseStatusCode { get; private set; }

    public string? ResponseContentType { get; private set; }

    public string? ResponseBody { get; private set; }

    public string? ResourceReference { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static IntegrationRequestRecord Create(
        Guid organizationId,
        Guid integrationClientId,
        string sourceSystem,
        string externalId,
        string eventType,
        string requestMethod,
        string requestPath,
        string requestHash,
        DateTimeOffset now) =>
        new(
            Guid.NewGuid(),
            organizationId,
            integrationClientId,
            sourceSystem,
            externalId,
            eventType,
            requestMethod,
            requestPath,
            requestHash,
            now);

    public void Complete(
        int responseStatusCode,
        string? responseContentType,
        string? responseBody,
        string? resourceReference,
        DateTimeOffset now)
    {
        if (Status != IntegrationRequestStatus.Processing)
        {
            throw new InvalidOperationException(
                "Only processing integration requests can be completed.");
        }

        if (responseStatusCode is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(
                nameof(responseStatusCode),
                "HTTP status code must be between 100 and 599.");
        }

        ResponseStatusCode = responseStatusCode;
        ResponseContentType = NormalizeOptional(
            responseContentType,
            256);
        ResponseBody = responseBody;
        ResourceReference = NormalizeOptional(
            resourceReference,
            2048);
        Status = IntegrationRequestStatus.Completed;
        CompletedAtUtc = now;
    }

    private static string NormalizeKeyPart(
        string value,
        string parameterName,
        int maxLength,
        bool toLower)
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

        return toLower
            ? normalized.ToLowerInvariant()
            : normalized;
    }

    private static string NormalizeHash(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant();

        if (normalized is null ||
            normalized.Length != 64 ||
            normalized.Any(character =>
                !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException(
                "Request hash must be a 64-character SHA-256 hex value.",
                nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                nameof(value));
        }

        return normalized;
    }
}
