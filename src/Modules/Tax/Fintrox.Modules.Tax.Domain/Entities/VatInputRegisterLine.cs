namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned VatInputRegisterLine; tenant scope and input fields validated at creation.</summary>
public sealed class VatInputRegisterLine : OrganizationEntity
{
    public VatInputRegisterLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceInvoiceId,
        string taxCode,
        decimal inputTaxAmount,
        decimal deductibleTaxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SourceInvoiceId = DomainRules.Id(sourceInvoiceId, nameof(sourceInvoiceId));
        TaxCode = DomainRules.Text(taxCode, nameof(taxCode));
        InputTaxAmount = DomainRules.Nonnegative(inputTaxAmount, nameof(inputTaxAmount));
        DeductibleTaxAmount = DomainRules.Nonnegative(deductibleTaxAmount, nameof(deductibleTaxAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SourceInvoiceId { get; }
    public string TaxCode { get; }
    public decimal InputTaxAmount { get; }
    public decimal DeductibleTaxAmount { get; }
    public string Currency { get; }
}
