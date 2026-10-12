namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderStatusHistory record.</summary>
public sealed class OrderStatusHistory : OrganizationEntity
{
    public OrderStatusHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentOrderId,
        string previousStatus,
        string newStatus,
        DateTimeOffset changedAtUtc,
        Guid changedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        PreviousStatus = DomainRules.Text(previousStatus, nameof(previousStatus));
        NewStatus = DomainRules.Text(newStatus, nameof(newStatus));
        ChangedAtUtc = DomainRules.Instant(changedAtUtc, nameof(changedAtUtc));
        ChangedByUserId = DomainRules.Id(changedByUserId, nameof(changedByUserId));
    }

    public Guid FulfillmentOrderId { get; }
    public string PreviousStatus { get; }
    public string NewStatus { get; }
    public DateTimeOffset ChangedAtUtc { get; }
    public Guid ChangedByUserId { get; }
}
