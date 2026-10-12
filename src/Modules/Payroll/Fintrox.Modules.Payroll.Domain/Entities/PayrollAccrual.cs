namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollAccrual; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollAccrual : OrganizationEntity
{
    public PayrollAccrual(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid payRunId,
        Guid accountingBookId,
        DateOnly accrualOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PayRunId = DomainRules.Id(payRunId, nameof(payRunId));
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        AccrualOn = DomainRules.Date(accrualOn, nameof(accrualOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PayRunId { get; }
    public Guid AccountingBookId { get; }
    public DateOnly AccrualOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
