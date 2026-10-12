namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned ReverseChargeEntry; tenant scope and input fields validated at creation.</summary>
public sealed class ReverseChargeEntry : OrganizationEntity
{
    public ReverseChargeEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceInvoiceId,
        Guid jurisdictionId,
        decimal taxableBase,
        decimal reverseTaxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SourceInvoiceId = DomainRules.Id(sourceInvoiceId, nameof(sourceInvoiceId));
        JurisdictionId = DomainRules.Id(jurisdictionId, nameof(jurisdictionId));
        TaxableBase = DomainRules.Nonnegative(taxableBase, nameof(taxableBase));
        ReverseTaxAmount = DomainRules.Nonnegative(reverseTaxAmount, nameof(reverseTaxAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SourceInvoiceId { get; }
    public Guid JurisdictionId { get; }
    public decimal TaxableBase { get; }
    public decimal ReverseTaxAmount { get; }
    public string Currency { get; }
}
