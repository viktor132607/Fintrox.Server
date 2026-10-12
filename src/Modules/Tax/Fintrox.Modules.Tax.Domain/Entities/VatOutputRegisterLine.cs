namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned VatOutputRegisterLine; tenant scope and input fields validated at creation.</summary>
public sealed class VatOutputRegisterLine : OrganizationEntity
{
    public VatOutputRegisterLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceInvoiceId,
        string taxCode,
        decimal taxableBase,
        decimal outputTaxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SourceInvoiceId = DomainRules.Id(sourceInvoiceId, nameof(sourceInvoiceId));
        TaxCode = DomainRules.Text(taxCode, nameof(taxCode));
        TaxableBase = DomainRules.Nonnegative(taxableBase, nameof(taxableBase));
        OutputTaxAmount = DomainRules.Nonnegative(outputTaxAmount, nameof(outputTaxAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SourceInvoiceId { get; }
    public string TaxCode { get; }
    public decimal TaxableBase { get; }
    public decimal OutputTaxAmount { get; }
    public string Currency { get; }
}
