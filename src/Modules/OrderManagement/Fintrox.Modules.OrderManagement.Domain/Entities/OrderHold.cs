namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderHold record.</summary>
public sealed class OrderHold : OrganizationEntity
{
    public OrderHold(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentOrderId,
        string holdType,
        string reason,
        Guid placedByUserId,
        DateTimeOffset placedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        HoldType = DomainRules.Text(holdType, nameof(holdType));
        Reason = DomainRules.Text(reason, nameof(reason));
        PlacedByUserId = DomainRules.Id(placedByUserId, nameof(placedByUserId));
        PlacedAtUtc = DomainRules.Instant(placedAtUtc, nameof(placedAtUtc));
    }

    public Guid FulfillmentOrderId { get; }
    public string HoldType { get; }
    public string Reason { get; }
    public Guid PlacedByUserId { get; }
    public DateTimeOffset PlacedAtUtc { get; }
}
