namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxReconciliation; tenant scope and input fields validated at creation.</summary>
public sealed class TaxReconciliation : OrganizationEntity
{
    public TaxReconciliation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid vatReturnPeriodId,
        Guid accountingBookId,
        DateOnly asOfDate,
        decimal registerTax,
        decimal ledgerTax,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        VatReturnPeriodId = DomainRules.Id(vatReturnPeriodId, nameof(vatReturnPeriodId));
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
        RegisterTax = DomainRules.Nonnegative(registerTax, nameof(registerTax));
        LedgerTax = DomainRules.Nonnegative(ledgerTax, nameof(ledgerTax));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid VatReturnPeriodId { get; }
    public Guid AccountingBookId { get; }
    public DateOnly AsOfDate { get; }
    public decimal RegisterTax { get; }
    public decimal LedgerTax { get; }
    public string Currency { get; }
}
