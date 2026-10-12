namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderRoutingDecision record.</summary>
public sealed class OrderRoutingDecision : OrganizationEntity
{
    public OrderRoutingDecision(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentOrderId,
        Guid routingRuleId,
        Guid selectedWarehouseId,
        DateTimeOffset decidedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        RoutingRuleId = DomainRules.Id(routingRuleId, nameof(routingRuleId));
        SelectedWarehouseId = DomainRules.Id(selectedWarehouseId, nameof(selectedWarehouseId));
        DecidedAtUtc = DomainRules.Instant(decidedAtUtc, nameof(decidedAtUtc));
    }

    public Guid FulfillmentOrderId { get; }
    public Guid RoutingRuleId { get; }
    public Guid SelectedWarehouseId { get; }
    public DateTimeOffset DecidedAtUtc { get; }
}
