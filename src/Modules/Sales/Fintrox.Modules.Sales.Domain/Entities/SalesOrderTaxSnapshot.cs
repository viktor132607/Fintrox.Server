namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesOrderTaxSnapshot model; no cross-module navigations.</summary>
public sealed class SalesOrderTaxSnapshot : OrganizationEntity
{
    public SalesOrderTaxSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderLineId,
        string taxCode,
        decimal taxableAmount,
        decimal taxPercent,
        decimal taxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderLineId = DomainRules.Id(salesOrderLineId, nameof(salesOrderLineId));
        TaxCode = DomainRules.Text(taxCode, nameof(taxCode));
        TaxableAmount = DomainRules.Nonnegative(taxableAmount, nameof(taxableAmount));
        TaxPercent = DomainRules.Percent(taxPercent, nameof(taxPercent));
        TaxAmount = DomainRules.Nonnegative(taxAmount, nameof(taxAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SalesOrderLineId { get; }
    public string TaxCode { get; }
    public decimal TaxableAmount { get; }
    public decimal TaxPercent { get; }
    public decimal TaxAmount { get; }
    public string Currency { get; }
}
