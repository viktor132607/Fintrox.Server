namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentProviderWebhook; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentProviderWebhook : OrganizationEntity
{
    public PaymentProviderWebhook(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid providerConnectionId,
        string eventType,
        string externalEventId,
        DateTimeOffset receivedAtUtc,
        string payloadHash)
        : base(id, organizationId, createdAtUtc)
    {
        ProviderConnectionId = DomainRules.Id(providerConnectionId, nameof(providerConnectionId));
        EventType = DomainRules.Text(eventType, nameof(eventType));
        ExternalEventId = DomainRules.Text(externalEventId, nameof(externalEventId));
        ReceivedAtUtc = DomainRules.Instant(receivedAtUtc, nameof(receivedAtUtc));
        PayloadHash = DomainRules.Hash(payloadHash, nameof(payloadHash));
    }

    public Guid ProviderConnectionId { get; }
    public string EventType { get; }
    public string ExternalEventId { get; }
    public DateTimeOffset ReceivedAtUtc { get; }
    public string PayloadHash { get; }
}
