namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesReturnAuthorization model; no cross-module navigations.</summary>
public sealed class SalesReturnAuthorization : OrganizationEntity
{
    public SalesReturnAuthorization(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        Guid customerId,
        string returnReference,
        DateOnly requestedOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        CustomerId = DomainRules.Id(customerId, nameof(customerId));
        ReturnReference = DomainRules.Text(returnReference, nameof(returnReference));
        RequestedOn = DomainRules.Date(requestedOn, nameof(requestedOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SalesOrderId { get; }
    public Guid CustomerId { get; }
    public string ReturnReference { get; }
    public DateOnly RequestedOn { get; }
    public string Status { get; }
}
