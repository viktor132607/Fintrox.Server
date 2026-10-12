namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetPeriod record. Domain invariants enforced at construction.</summary>
public sealed class BudgetPeriod : OrganizationEntity
{
    public BudgetPeriod(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetId,
        int periodNumber,
        DateOnly startsOn,
        DateOnly endsOn,
        bool isLocked)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetId = DomainRules.Id(budgetId, nameof(budgetId));
        PeriodNumber = DomainRules.PositiveInt(periodNumber, nameof(periodNumber));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        IsLocked = isLocked;
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid BudgetId { get; }
    public int PeriodNumber { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public bool IsLocked { get; }
}
