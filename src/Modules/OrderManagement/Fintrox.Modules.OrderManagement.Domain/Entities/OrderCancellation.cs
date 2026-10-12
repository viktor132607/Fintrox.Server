namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderCancellation record.</summary>
public sealed class OrderCancellation : OrganizationEntity
{
    public OrderCancellation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentOrderId,
        DateTimeOffset cancelledAtUtc,
        Guid cancelledByUserId,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        CancelledAtUtc = DomainRules.Instant(cancelledAtUtc, nameof(cancelledAtUtc));
        CancelledByUserId = DomainRules.Id(cancelledByUserId, nameof(cancelledByUserId));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid FulfillmentOrderId { get; }
    public DateTimeOffset CancelledAtUtc { get; }
    public Guid CancelledByUserId { get; }
    public string Reason { get; }
}
