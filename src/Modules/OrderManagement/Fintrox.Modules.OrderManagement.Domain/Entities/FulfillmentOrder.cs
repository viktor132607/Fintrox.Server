namespace Fintrox.Modules.OrderManagement.Domain.Entities;

public sealed class FulfillmentOrder : OrganizationEntity
{
    public FulfillmentOrder(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        string reference)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        Reference = DomainRules.Text(reference, nameof(reference));
    }

    public Guid SalesOrderId { get; }
    public string Reference { get; }
}
