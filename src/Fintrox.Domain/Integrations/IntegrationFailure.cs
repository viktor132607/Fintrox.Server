using Fintrox.Domain.Common;

namespace Fintrox.Domain.Integrations;

public sealed class IntegrationFailure
    : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private IntegrationFailure()
    {
    }

    private IntegrationFailure(
        Guid id,
        Guid organizationId,
        IntegrationFailureKind kind,
        Guid referenceId,
        string reason,
        string payloadJson,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        if (referenceId == Guid.Empty)
        {
            throw new ArgumentException(
                "Failure reference id is required.",
                nameof(referenceId));
        }

        Kind = kind;
        ReferenceId = referenceId;
        Reason = NormalizeReason(reason);
        PayloadJson = string.IsNullOrWhiteSpace(payloadJson)
            ? "{}"
            : payloadJson.Trim();
        RetryCount = 1;
        LastAttemptAtUtc = now;
    }

    public IntegrationFailureKind Kind { get; private set; }

    public Guid ReferenceId { get; private set; }

    public string Reason { get; private set; } = null!;

    public string PayloadJson { get; private set; } = null!;

    public int RetryCount { get; private set; }

    public DateTimeOffset LastAttemptAtUtc { get; private set; }

    public bool IsResolved { get; private set; }

    public DateTimeOffset? ResolvedAtUtc { get; private set; }

    public static IntegrationFailure Create(
        Guid organizationId,
        IntegrationFailureKind kind,
        Guid referenceId,
        string reason,
        string payloadJson,
        DateTimeOffset now) =>
        new(
            Guid.NewGuid(),
            organizationId,
            kind,
            referenceId,
            reason,
            payloadJson,
            now);

    public void RecordRetry(
        string reason,
        string payloadJson,
        DateTimeOffset now)
    {
        if (IsResolved)
        {
            throw new InvalidOperationException(
                "A resolved integration failure cannot record another retry.");
        }

        Reason = NormalizeReason(reason);
        PayloadJson = string.IsNullOrWhiteSpace(payloadJson)
            ? "{}"
            : payloadJson.Trim();
        RetryCount++;
        LastAttemptAtUtc = now;
        Touch(now);
    }

    public void Resolve(DateTimeOffset now)
    {
        if (IsResolved)
        {
            return;
        }

        IsResolved = true;
        ResolvedAtUtc = now;
        Touch(now);
    }

    private static string NormalizeReason(string value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "Failure reason is required.",
                nameof(value));
        }

        return normalized.Length <= 4000
            ? normalized
            : normalized[..4000];
    }
}
