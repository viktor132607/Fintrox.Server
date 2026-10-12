namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesReturnLine model; no cross-module navigations.</summary>
public sealed class SalesReturnLine : OrganizationEntity
{
    public SalesReturnLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid returnAuthorizationId,
        Guid salesOrderLineId,
        decimal quantity,
        string returnReason)
        : base(id, organizationId, createdAtUtc)
    {
        ReturnAuthorizationId = DomainRules.Id(returnAuthorizationId, nameof(returnAuthorizationId));
        SalesOrderLineId = DomainRules.Id(salesOrderLineId, nameof(salesOrderLineId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        ReturnReason = DomainRules.Text(returnReason, nameof(returnReason));
    }

    public Guid ReturnAuthorizationId { get; }
    public Guid SalesOrderLineId { get; }
    public decimal Quantity { get; }
    public string ReturnReason { get; }
}
