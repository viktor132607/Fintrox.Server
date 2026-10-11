namespace Fintrox.Modules.OrderManagement.Domain.Entities;

public sealed class Backorder : OrganizationEntity
{
    public Backorder(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderLineId,
        decimal quantity,
        DateOnly expectedOn)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderLineId = DomainRules.Id(salesOrderLineId, nameof(salesOrderLineId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        ExpectedOn = DomainRules.Date(expectedOn, nameof(expectedOn));
    }

    public Guid SalesOrderLineId { get; }
    public decimal Quantity { get; }
    public DateOnly ExpectedOn { get; }
}
