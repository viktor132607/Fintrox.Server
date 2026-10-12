namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetScenario record. Domain invariants enforced at construction.</summary>
public sealed class BudgetScenario : OrganizationEntity
{
    public BudgetScenario(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetId,
        string name,
        string scenarioType,
        decimal probabilityPercent,
        bool isBaseline)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetId = DomainRules.Id(budgetId, nameof(budgetId));
        Name = DomainRules.Text(name, nameof(name));
        ScenarioType = DomainRules.Text(scenarioType, nameof(scenarioType));
        ProbabilityPercent = DomainRules.Percent(probabilityPercent, nameof(probabilityPercent));
        IsBaseline = isBaseline;
    }

    public Guid BudgetId { get; }
    public string Name { get; }
    public string ScenarioType { get; }
    public decimal ProbabilityPercent { get; }
    public bool IsBaseline { get; }
}
