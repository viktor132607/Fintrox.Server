namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetDimension record. Domain invariants enforced at construction.</summary>
public sealed class BudgetDimension : OrganizationEntity
{
    public BudgetDimension(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetId,
        string dimensionCode,
        Guid dimensionValueId,
        bool isRequired)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetId = DomainRules.Id(budgetId, nameof(budgetId));
        DimensionCode = DomainRules.Text(dimensionCode, nameof(dimensionCode));
        DimensionValueId = DomainRules.Id(dimensionValueId, nameof(dimensionValueId));
        IsRequired = isRequired;
    }

    public Guid BudgetId { get; }
    public string DimensionCode { get; }
    public Guid DimensionValueId { get; }
    public bool IsRequired { get; }
}
