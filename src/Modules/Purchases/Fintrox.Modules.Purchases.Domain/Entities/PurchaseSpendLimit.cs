namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseSpendLimit model. References require application validation.</summary>
public sealed class PurchaseSpendLimit : OrganizationEntity
{
    public PurchaseSpendLimit(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid spendCategoryId,
        Guid departmentId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal maximumAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SpendCategoryId = DomainRules.Id(spendCategoryId, nameof(spendCategoryId));
        DepartmentId = DomainRules.Id(departmentId, nameof(departmentId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        MaximumAmount = DomainRules.Nonnegative(maximumAmount, nameof(maximumAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid SpendCategoryId { get; }
    public Guid DepartmentId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal MaximumAmount { get; }
    public string Currency { get; }
}
