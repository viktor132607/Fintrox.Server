namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetActualSnapshot record. Domain invariants enforced at construction.</summary>
public sealed class BudgetActualSnapshot : OrganizationEntity
{
    public BudgetActualSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetLineId,
        DateOnly asOfDate,
        decimal actualAmount,
        decimal committedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetLineId = DomainRules.Id(budgetLineId, nameof(budgetLineId));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
        ActualAmount = DomainRules.Nonnegative(actualAmount, nameof(actualAmount));
        CommittedAmount = DomainRules.Nonnegative(committedAmount, nameof(committedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid BudgetLineId { get; }
    public DateOnly AsOfDate { get; }
    public decimal ActualAmount { get; }
    public decimal CommittedAmount { get; }
    public string Currency { get; }
}
