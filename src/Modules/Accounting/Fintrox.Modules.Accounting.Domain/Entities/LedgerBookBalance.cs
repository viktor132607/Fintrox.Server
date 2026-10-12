namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped LedgerBookBalance domain record; referenced objects are validated by application use cases.</summary>
public sealed class LedgerBookBalance : OrganizationEntity
{
    public LedgerBookBalance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid accountingBookId,
        Guid accountId,
        DateOnly asOfDate,
        decimal debitTotal,
        decimal creditTotal,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
        DebitTotal = DomainRules.Nonnegative(debitTotal, nameof(debitTotal));
        CreditTotal = DomainRules.Nonnegative(creditTotal, nameof(creditTotal));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid AccountingBookId { get; }
    public Guid AccountId { get; }
    public DateOnly AsOfDate { get; }
    public decimal DebitTotal { get; }
    public decimal CreditTotal { get; }
    public string Currency { get; }
}
