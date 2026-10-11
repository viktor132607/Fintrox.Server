namespace Fintrox.Modules.OrderManagement.Domain.Entities;

public sealed class ReturnLine : OrganizationEntity
{
    public ReturnLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid returnAuthorizationId,
        Guid salesOrderLineId,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        ReturnAuthorizationId = DomainRules.Id(returnAuthorizationId, nameof(returnAuthorizationId));
        SalesOrderLineId = DomainRules.Id(salesOrderLineId, nameof(salesOrderLineId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid ReturnAuthorizationId { get; }
    public Guid SalesOrderLineId { get; }
    public decimal Quantity { get; }
}
