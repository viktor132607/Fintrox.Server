namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowCondition; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowCondition : OrganizationEntity
{
    public WorkflowCondition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowVersionId,
        string conditionCode,
        string expression,
        int priority)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowVersionId = DomainRules.Id(workflowVersionId, nameof(workflowVersionId));
        ConditionCode = DomainRules.Text(conditionCode, nameof(conditionCode));
        Expression = DomainRules.Text(expression, nameof(expression));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
    }

    public Guid WorkflowVersionId { get; }
    public string ConditionCode { get; }
    public string Expression { get; }
    public int Priority { get; }
}
