namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectExpenseAllocation; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectExpenseAllocation : OrganizationEntity
{
    public ProjectExpenseAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectExpenseId,
        Guid costCenterId,
        decimal amount,
        string currency,
        DateOnly allocatedOn)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectExpenseId = DomainRules.Id(projectExpenseId, nameof(projectExpenseId));
        CostCenterId = DomainRules.Id(costCenterId, nameof(costCenterId));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        AllocatedOn = DomainRules.Date(allocatedOn, nameof(allocatedOn));
    }

    public Guid ProjectExpenseId { get; }
    public Guid CostCenterId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public DateOnly AllocatedOn { get; }
}
