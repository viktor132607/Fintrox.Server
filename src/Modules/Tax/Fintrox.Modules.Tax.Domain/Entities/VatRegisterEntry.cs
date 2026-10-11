namespace Fintrox.Modules.Tax.Domain.Entities;

public sealed class VatRegisterEntry : OrganizationEntity
{
    public VatRegisterEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid periodId,
        Guid sourceDocumentId,
        VatRegisterKind kind,
        decimal taxableAmount,
        decimal taxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PeriodId = DomainRules.Id(periodId, nameof(periodId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        Kind = DomainRules.Defined(kind, nameof(kind));
        TaxableAmount = taxableAmount;
        TaxAmount = taxAmount;
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PeriodId { get; }
    public Guid SourceDocumentId { get; }
    public VatRegisterKind Kind { get; }
    public decimal TaxableAmount { get; }
    public decimal TaxAmount { get; }
    public string Currency { get; }
}
