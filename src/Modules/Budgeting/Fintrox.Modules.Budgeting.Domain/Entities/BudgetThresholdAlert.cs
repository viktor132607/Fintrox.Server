namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetThresholdAlert record. Domain invariants enforced at construction.</summary>
public sealed class BudgetThresholdAlert : OrganizationEntity
{
    public BudgetThresholdAlert(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetLineId,
        decimal thresholdPercent,
        DateTimeOffset triggeredAtUtc,
        decimal actualAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetLineId = DomainRules.Id(budgetLineId, nameof(budgetLineId));
        ThresholdPercent = DomainRules.Percent(thresholdPercent, nameof(thresholdPercent));
        TriggeredAtUtc = DomainRules.Instant(triggeredAtUtc, nameof(triggeredAtUtc));
        ActualAmount = DomainRules.Nonnegative(actualAmount, nameof(actualAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid BudgetLineId { get; }
    public decimal ThresholdPercent { get; }
    public DateTimeOffset TriggeredAtUtc { get; }
    public decimal ActualAmount { get; }
    public string Currency { get; }
}
