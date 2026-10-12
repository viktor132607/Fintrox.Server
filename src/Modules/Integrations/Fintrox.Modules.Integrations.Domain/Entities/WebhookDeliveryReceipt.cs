namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned WebhookDeliveryReceipt domain record; references checked in application layer.</summary>
public sealed class WebhookDeliveryReceipt : OrganizationEntity
{
    public WebhookDeliveryReceipt(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid deliveryAttemptId,
        DateTimeOffset receivedAtUtc,
        string acknowledgmentCode,
        bool isAccepted)
        : base(id, organizationId, createdAtUtc)
    {
        DeliveryAttemptId = DomainRules.Id(deliveryAttemptId, nameof(deliveryAttemptId));
        ReceivedAtUtc = DomainRules.Instant(receivedAtUtc, nameof(receivedAtUtc));
        AcknowledgmentCode = DomainRules.Text(acknowledgmentCode, nameof(acknowledgmentCode));
        IsAccepted = isAccepted;
    }

    public Guid DeliveryAttemptId { get; }
    public DateTimeOffset ReceivedAtUtc { get; }
    public string AcknowledgmentCode { get; }
    public bool IsAccepted { get; }
}
