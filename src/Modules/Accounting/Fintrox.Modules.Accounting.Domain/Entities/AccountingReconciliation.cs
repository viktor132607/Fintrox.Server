namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped AccountingReconciliation domain record; referenced objects are validated by application use cases.</summary>
public sealed class AccountingReconciliation : OrganizationEntity
{
    public AccountingReconciliation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid accountingBookId,
        Guid accountId,
        DateOnly statementDate,
        decimal expectedBalance,
        decimal actualBalance,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        StatementDate = DomainRules.Date(statementDate, nameof(statementDate));
        ExpectedBalance = DomainRules.Nonnegative(expectedBalance, nameof(expectedBalance));
        ActualBalance = DomainRules.Nonnegative(actualBalance, nameof(actualBalance));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid AccountingBookId { get; }
    public Guid AccountId { get; }
    public DateOnly StatementDate { get; }
    public decimal ExpectedBalance { get; }
    public decimal ActualBalance { get; }
    public string Currency { get; }
}
