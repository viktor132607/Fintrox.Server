namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetOwner record. Domain invariants enforced at construction.</summary>
public sealed class BudgetOwner : OrganizationEntity
{
    public BudgetOwner(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetId,
        Guid responsibleUserId,
        string responsibilityType,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetId = DomainRules.Id(budgetId, nameof(budgetId));
        ResponsibleUserId = DomainRules.Id(responsibleUserId, nameof(responsibleUserId));
        ResponsibilityType = DomainRules.Text(responsibilityType, nameof(responsibilityType));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid BudgetId { get; }
    public Guid ResponsibleUserId { get; }
    public string ResponsibilityType { get; }
    public DateOnly EffectiveFrom { get; }
}
