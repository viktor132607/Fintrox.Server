using Fintrox.Domain.Common;

namespace Fintrox.Domain.Integrations;

public sealed class IntegrationEvent
    : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private IntegrationEvent()
    {
    }

    private IntegrationEvent(
        Guid id,
        Guid organizationId,
        Guid? inboxId,
        string eventType,
        string aggregateType,
        Guid aggregateId,
        string payloadJson,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        if (aggregateId == Guid.Empty)
        {
            throw new ArgumentException(
                "Aggregate id is required.",
                nameof(aggregateId));
        }

        InboxId = inboxId;
        EventType = NormalizeRequired(
            eventType,
            nameof(eventType),
            120,
            toLower: true);
        AggregateType = NormalizeRequired(
            aggregateType,
            nameof(aggregateType),
            100,
            toLower: false);
        AggregateId = aggregateId;
        PayloadJson = string.IsNullOrWhiteSpace(payloadJson)
            ? "{}"
            : payloadJson.Trim();
        OccurredAtUtc = occurredAtUtc;
        Status = IntegrationEventStatus.Pending;
    }

    public Guid? InboxId { get; private set; }

    public string EventType { get; private set; } = null!;

    public string AggregateType { get; private set; } = null!;

    public Guid AggregateId { get; private set; }

    public string PayloadJson { get; private set; } = null!;

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public IntegrationEventStatus Status { get; private set; }

    public DateTimeOffset? DispatchedAtUtc { get; private set; }

    public static IntegrationEvent Create(
        Guid organizationId,
        Guid? inboxId,
        string eventType,
        string aggregateType,
        Guid aggregateId,
        string payloadJson,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset now) =>
        new(
            Guid.NewGuid(),
            organizationId,
            inboxId,
            eventType,
            aggregateType,
            aggregateId,
            payloadJson,
            occurredAtUtc,
            now);

    public void MarkDispatched(DateTimeOffset now)
    {
        if (Status == IntegrationEventStatus.Dispatched)
        {
            return;
        }

        Status = IntegrationEventStatus.Dispatched;
        DispatchedAtUtc = now;
        Touch(now);
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
