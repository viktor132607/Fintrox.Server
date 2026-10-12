namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetCommitment record. Domain invariants enforced at construction.</summary>
public sealed class BudgetCommitment : OrganizationEntity
{
    public BudgetCommitment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetLineId,
        Guid sourceDocumentId,
        DateOnly committedOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetLineId = DomainRules.Id(budgetLineId, nameof(budgetLineId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        CommittedOn = DomainRules.Date(committedOn, nameof(committedOn));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid BudgetLineId { get; }
    public Guid SourceDocumentId { get; }
    public DateOnly CommittedOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
