namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned VatInvoiceRegistration; tenant scope and input fields validated at creation.</summary>
public sealed class VatInvoiceRegistration : OrganizationEntity
{
    public VatInvoiceRegistration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceInvoiceId,
        Guid vatRegisterPeriodId,
        DateOnly taxPointOn,
        decimal taxableBase,
        decimal taxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SourceInvoiceId = DomainRules.Id(sourceInvoiceId, nameof(sourceInvoiceId));
        VatRegisterPeriodId = DomainRules.Id(vatRegisterPeriodId, nameof(vatRegisterPeriodId));
        TaxPointOn = DomainRules.Date(taxPointOn, nameof(taxPointOn));
        TaxableBase = DomainRules.Nonnegative(taxableBase, nameof(taxableBase));
        TaxAmount = DomainRules.Nonnegative(taxAmount, nameof(taxAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SourceInvoiceId { get; }
    public Guid VatRegisterPeriodId { get; }
    public DateOnly TaxPointOn { get; }
    public decimal TaxableBase { get; }
    public decimal TaxAmount { get; }
    public string Currency { get; }
}
