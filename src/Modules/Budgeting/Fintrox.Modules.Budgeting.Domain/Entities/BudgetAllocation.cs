namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetAllocation record. Domain invariants enforced at construction.</summary>
public sealed class BudgetAllocation : OrganizationEntity
{
    public BudgetAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetVersionId,
        Guid sourceLineId,
        Guid targetLineId,
        decimal allocatedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetVersionId = DomainRules.Id(budgetVersionId, nameof(budgetVersionId));
        SourceLineId = DomainRules.Id(sourceLineId, nameof(sourceLineId));
        TargetLineId = DomainRules.Id(targetLineId, nameof(targetLineId));
        AllocatedAmount = DomainRules.Nonnegative(allocatedAmount, nameof(allocatedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid BudgetVersionId { get; }
    public Guid SourceLineId { get; }
    public Guid TargetLineId { get; }
    public decimal AllocatedAmount { get; }
    public string Currency { get; }
}
