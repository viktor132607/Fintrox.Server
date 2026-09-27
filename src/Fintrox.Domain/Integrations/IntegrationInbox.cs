using Fintrox.Domain.Common;

namespace Fintrox.Domain.Integrations;

public sealed class IntegrationInbox
    : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private IntegrationInbox()
    {
    }

    private IntegrationInbox(
        Guid id,
        Guid organizationId,
        string sourceSystem,
        string externalId,
        string eventType,
        string operation,
        string payloadJson,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        SourceSystem = NormalizeRequired(
            sourceSystem,
            nameof(sourceSystem),
            100,
            toLower: true);
        ExternalId = NormalizeRequired(
            externalId,
            nameof(externalId),
            200,
            toLower: false);
        EventType = NormalizeRequired(
            eventType,
            nameof(eventType),
            100,
            toLower: true);
        Operation = NormalizeRequired(
            operation,
            nameof(operation),
            64,
            toLower: true);
        PayloadJson = NormalizePayload(payloadJson);
        Status = IntegrationInboxStatus.Received;
    }

    public string SourceSystem { get; private set; } = null!;

    public string ExternalId { get; private set; } = null!;

    public string EventType { get; private set; } = null!;

    public string Operation { get; private set; } = null!;

    public string PayloadJson { get; private set; } = null!;

    public IntegrationInboxStatus Status { get; private set; }

    public int RetryCount { get; private set; }

    public DateTimeOffset? LastAttemptAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public string? ResultEntityType { get; private set; }

    public Guid? ResultEntityId { get; private set; }

    public Guid? JournalEntryId { get; private set; }

    public string? FailureReason { get; private set; }

    public static IntegrationInbox Create(
        Guid organizationId,
        string sourceSystem,
        string externalId,
        string eventType,
        string operation,
        string payloadJson,
        DateTimeOffset now) =>
        new(
            Guid.NewGuid(),
            organizationId,
            sourceSystem,
            externalId,
            eventType,
            operation,
            payloadJson,
            now);

    public void StartProcessing(DateTimeOffset now)
    {
        if (Status == IntegrationInboxStatus.Succeeded)
        {
            throw new InvalidOperationException(
                "A successful inbox item cannot be processed again.");
        }

        Status = IntegrationInboxStatus.Processing;
        FailureReason = null;
        LastAttemptAtUtc = now;
        Touch(now);
    }

    public void Succeed(
        string resultEntityType,
        Guid resultEntityId,
        Guid? journalEntryId,
        DateTimeOffset now)
    {
        if (Status != IntegrationInboxStatus.Processing)
        {
            throw new InvalidOperationException(
                "Only a processing inbox item can succeed.");
        }

        if (resultEntityId == Guid.Empty)
        {
            throw new ArgumentException(
                "Result entity id is required.",
                nameof(resultEntityId));
        }

        ResultEntityType = NormalizeRequired(
            resultEntityType,
            nameof(resultEntityType),
            100,
            toLower: false);
        ResultEntityId = resultEntityId;
        JournalEntryId = journalEntryId;
        Status = IntegrationInboxStatus.Succeeded;
        CompletedAtUtc = now;
        FailureReason = null;
        Touch(now);
    }

    public void Fail(string reason, DateTimeOffset now)
    {
        if (Status == IntegrationInboxStatus.Succeeded)
        {
            throw new InvalidOperationException(
                "A successful inbox item cannot fail.");
        }

        FailureReason = NormalizeRequired(
            reason,
            nameof(reason),
            4000,
            toLower: false);
        RetryCount++;
        LastAttemptAtUtc = now;
        Status = IntegrationInboxStatus.Failed;
        CompletedAtUtc = null;
        Touch(now);
    }

    private static string NormalizePayload(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Payload JSON is required.",
                nameof(value));
        }

        return value.Trim();
    }

    private static string NormalizeRequired(
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
}
