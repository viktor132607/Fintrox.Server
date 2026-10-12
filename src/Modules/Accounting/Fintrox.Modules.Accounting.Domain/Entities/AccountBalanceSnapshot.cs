namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped AccountBalanceSnapshot domain record; referenced objects are validated by application use cases.</summary>
public sealed class AccountBalanceSnapshot : OrganizationEntity
{
    public AccountBalanceSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid accountId,
        Guid accountingBookId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal openingDebit,
        decimal openingCredit,
        decimal movementDebit,
        decimal movementCredit,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        OpeningDebit = DomainRules.Nonnegative(openingDebit, nameof(openingDebit));
        OpeningCredit = DomainRules.Nonnegative(openingCredit, nameof(openingCredit));
        MovementDebit = DomainRules.Nonnegative(movementDebit, nameof(movementDebit));
        MovementCredit = DomainRules.Nonnegative(movementCredit, nameof(movementCredit));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid AccountId { get; }
    public Guid AccountingBookId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal OpeningDebit { get; }
    public decimal OpeningCredit { get; }
    public decimal MovementDebit { get; }
    public decimal MovementCredit { get; }
    public string Currency { get; }
}
