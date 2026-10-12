namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesOrderPriceOverride model; no cross-module navigations.</summary>
public sealed class SalesOrderPriceOverride : OrganizationEntity
{
    public SalesOrderPriceOverride(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderLineId,
        decimal originalPrice,
        decimal overridePrice,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderLineId = DomainRules.Id(salesOrderLineId, nameof(salesOrderLineId));
        OriginalPrice = DomainRules.Nonnegative(originalPrice, nameof(originalPrice));
        OverridePrice = DomainRules.Nonnegative(overridePrice, nameof(overridePrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid SalesOrderLineId { get; }
    public decimal OriginalPrice { get; }
    public decimal OverridePrice { get; }
    public string Currency { get; }
    public string Reason { get; }
}
