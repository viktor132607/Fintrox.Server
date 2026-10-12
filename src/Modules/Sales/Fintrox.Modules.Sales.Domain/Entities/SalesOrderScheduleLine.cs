namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesOrderScheduleLine model; no cross-module navigations.</summary>
public sealed class SalesOrderScheduleLine : OrganizationEntity
{
    public SalesOrderScheduleLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderLineId,
        DateOnly promisedOn,
        decimal quantity,
        Guid warehouseId)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderLineId = DomainRules.Id(salesOrderLineId, nameof(salesOrderLineId));
        PromisedOn = DomainRules.Date(promisedOn, nameof(promisedOn));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
    }

    public Guid SalesOrderLineId { get; }
    public DateOnly PromisedOn { get; }
    public decimal Quantity { get; }
    public Guid WarehouseId { get; }
}
