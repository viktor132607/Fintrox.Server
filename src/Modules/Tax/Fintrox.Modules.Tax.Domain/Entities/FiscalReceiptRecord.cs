namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned FiscalReceiptRecord; tenant scope and input fields validated at creation.</summary>
public sealed class FiscalReceiptRecord : OrganizationEntity
{
    public FiscalReceiptRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceSaleId,
        string fiscalReceiptNumber,
        DateTimeOffset issuedAtUtc,
        decimal taxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SourceSaleId = DomainRules.Id(sourceSaleId, nameof(sourceSaleId));
        FiscalReceiptNumber = DomainRules.Text(fiscalReceiptNumber, nameof(fiscalReceiptNumber));
        IssuedAtUtc = DomainRules.Instant(issuedAtUtc, nameof(issuedAtUtc));
        TaxAmount = DomainRules.Nonnegative(taxAmount, nameof(taxAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SourceSaleId { get; }
    public string FiscalReceiptNumber { get; }
    public DateTimeOffset IssuedAtUtc { get; }
    public decimal TaxAmount { get; }
    public string Currency { get; }
}
