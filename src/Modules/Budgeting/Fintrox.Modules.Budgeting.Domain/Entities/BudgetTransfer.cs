namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetTransfer record. Domain invariants enforced at construction.</summary>
public sealed class BudgetTransfer : OrganizationEntity
{
    public BudgetTransfer(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceBudgetLineId,
        Guid targetBudgetLineId,
        decimal transferAmount,
        string currency,
        DateTimeOffset requestedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SourceBudgetLineId = DomainRules.Id(sourceBudgetLineId, nameof(sourceBudgetLineId));
        TargetBudgetLineId = DomainRules.Id(targetBudgetLineId, nameof(targetBudgetLineId));
        TransferAmount = DomainRules.Positive(transferAmount, nameof(transferAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
    }

    public Guid SourceBudgetLineId { get; }
    public Guid TargetBudgetLineId { get; }
    public decimal TransferAmount { get; }
    public string Currency { get; }
    public DateTimeOffset RequestedAtUtc { get; }
}
