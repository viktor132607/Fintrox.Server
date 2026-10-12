namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesOrderFulfillmentLink model; no cross-module navigations.</summary>
public sealed class SalesOrderFulfillmentLink : OrganizationEntity
{
    public SalesOrderFulfillmentLink(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderLineId,
        Guid fulfillmentOrderId,
        decimal quantity,
        DateOnly linkedOn)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderLineId = DomainRules.Id(salesOrderLineId, nameof(salesOrderLineId));
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        LinkedOn = DomainRules.Date(linkedOn, nameof(linkedOn));
    }

    public Guid SalesOrderLineId { get; }
    public Guid FulfillmentOrderId { get; }
    public decimal Quantity { get; }
    public DateOnly LinkedOn { get; }
}
