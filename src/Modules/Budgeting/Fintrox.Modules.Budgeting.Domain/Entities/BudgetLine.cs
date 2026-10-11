namespace Fintrox.Modules.Budgeting.Domain.Entities;

public sealed class BudgetLine : OrganizationEntity
{
    public BudgetLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetVersionId,
        Guid accountId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetVersionId = DomainRules.Id(budgetVersionId, nameof(budgetVersionId));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(periodEndsOn));
    }

    public Guid BudgetVersionId { get; }
    public Guid AccountId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
