namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderSplit record.</summary>
public sealed class OrderSplit : OrganizationEntity
{
    public OrderSplit(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceFulfillmentOrderId,
        Guid childFulfillmentOrderId,
        string splitReason,
        DateTimeOffset splitAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SourceFulfillmentOrderId = DomainRules.Id(sourceFulfillmentOrderId, nameof(sourceFulfillmentOrderId));
        ChildFulfillmentOrderId = DomainRules.Id(childFulfillmentOrderId, nameof(childFulfillmentOrderId));
        SplitReason = DomainRules.Text(splitReason, nameof(splitReason));
        SplitAtUtc = DomainRules.Instant(splitAtUtc, nameof(splitAtUtc));
    }

    public Guid SourceFulfillmentOrderId { get; }
    public Guid ChildFulfillmentOrderId { get; }
    public string SplitReason { get; }
    public DateTimeOffset SplitAtUtc { get; }
}
