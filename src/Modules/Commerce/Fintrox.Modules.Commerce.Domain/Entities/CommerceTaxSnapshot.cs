namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned CommerceTaxSnapshot; external references validated by application layer.</summary>
public sealed class CommerceTaxSnapshot : OrganizationEntity
{
    public CommerceTaxSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid orderLineId,
        string taxCode,
        decimal taxRatePercent,
        decimal taxableAmount,
        decimal taxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        OrderLineId = DomainRules.Id(orderLineId, nameof(orderLineId));
        TaxCode = DomainRules.Text(taxCode, nameof(taxCode));
        TaxRatePercent = DomainRules.Percent(taxRatePercent, nameof(taxRatePercent));
        TaxableAmount = DomainRules.Nonnegative(taxableAmount, nameof(taxableAmount));
        TaxAmount = DomainRules.Nonnegative(taxAmount, nameof(taxAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid OrderLineId { get; }
    public string TaxCode { get; }
    public decimal TaxRatePercent { get; }
    public decimal TaxableAmount { get; }
    public decimal TaxAmount { get; }
    public string Currency { get; }
}
