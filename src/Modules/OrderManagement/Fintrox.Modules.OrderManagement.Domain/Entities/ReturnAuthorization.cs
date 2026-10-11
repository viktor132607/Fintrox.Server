namespace Fintrox.Modules.OrderManagement.Domain.Entities;

public sealed class ReturnAuthorization : OrganizationEntity
{
    public ReturnAuthorization(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        string reference,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        Reference = DomainRules.Text(reference, nameof(reference));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid SalesOrderId { get; }
    public string Reference { get; }
    public string Reason { get; }
}
