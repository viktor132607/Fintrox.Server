namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectCostBudget; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectCostBudget : OrganizationEntity
{
    public ProjectCostBudget(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        string costCategory,
        decimal approvedAmount,
        string currency,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        CostCategory = DomainRules.Text(costCategory, nameof(costCategory));
        ApprovedAmount = DomainRules.Nonnegative(approvedAmount, nameof(approvedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid ProjectId { get; }
    public string CostCategory { get; }
    public decimal ApprovedAmount { get; }
    public string Currency { get; }
    public DateOnly EffectiveFrom { get; }
}
