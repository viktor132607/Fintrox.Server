namespace Fintrox.Modules.Sales.Domain.Entities;

public sealed class SalesOrderLine : OrganizationEntity
{
    public SalesOrderLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        Guid itemId,
        decimal quantity,
        decimal unitPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SalesOrderId { get; }
    public Guid ItemId { get; }
    public decimal Quantity { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
}
