namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetAdjustment record. Domain invariants enforced at construction.</summary>
public sealed class BudgetAdjustment : OrganizationEntity
{
    public BudgetAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetLineId,
        string reason,
        decimal adjustmentAmount,
        string currency,
        Guid approvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetLineId = DomainRules.Id(budgetLineId, nameof(budgetLineId));
        Reason = DomainRules.Text(reason, nameof(reason));
        AdjustmentAmount = DomainRules.Nonzero(adjustmentAmount, nameof(adjustmentAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
    }

    public Guid BudgetLineId { get; }
    public string Reason { get; }
    public decimal AdjustmentAmount { get; }
    public string Currency { get; }
    public Guid ApprovedByUserId { get; }
}
