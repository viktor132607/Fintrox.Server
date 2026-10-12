namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetApproval record. Domain invariants enforced at construction.</summary>
public sealed class BudgetApproval : OrganizationEntity
{
    public BudgetApproval(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetVersionId,
        Guid approverUserId,
        string decision,
        DateTimeOffset decidedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetVersionId = DomainRules.Id(budgetVersionId, nameof(budgetVersionId));
        ApproverUserId = DomainRules.Id(approverUserId, nameof(approverUserId));
        Decision = DomainRules.Text(decision, nameof(decision));
        DecidedAtUtc = DomainRules.Instant(decidedAtUtc, nameof(decidedAtUtc));
    }

    public Guid BudgetVersionId { get; }
    public Guid ApproverUserId { get; }
    public string Decision { get; }
    public DateTimeOffset DecidedAtUtc { get; }
}
