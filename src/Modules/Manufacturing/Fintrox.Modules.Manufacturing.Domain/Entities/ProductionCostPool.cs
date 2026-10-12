namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionCostPool record.</summary>
public sealed class ProductionCostPool : OrganizationEntity
{
    public ProductionCostPool(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workCenterId,
        string costCategory,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        WorkCenterId = DomainRules.Id(workCenterId, nameof(workCenterId));
        CostCategory = DomainRules.Text(costCategory, nameof(costCategory));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid WorkCenterId { get; }
    public string CostCategory { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
