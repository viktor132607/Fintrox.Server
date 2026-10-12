namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetVariance record. Domain invariants enforced at construction.</summary>
public sealed class BudgetVariance : OrganizationEntity
{
    public BudgetVariance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetLineId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal budgetAmount,
        decimal actualAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetLineId = DomainRules.Id(budgetLineId, nameof(budgetLineId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        BudgetAmount = DomainRules.Nonnegative(budgetAmount, nameof(budgetAmount));
        ActualAmount = DomainRules.Nonnegative(actualAmount, nameof(actualAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid BudgetLineId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal BudgetAmount { get; }
    public decimal ActualAmount { get; }
    public string Currency { get; }
}
