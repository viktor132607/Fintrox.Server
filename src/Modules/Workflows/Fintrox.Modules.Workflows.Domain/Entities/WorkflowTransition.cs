namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowTransition; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowTransition : OrganizationEntity
{
    public WorkflowTransition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowVersionId,
        string fromState,
        string toState,
        Guid conditionId,
        int priority)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowVersionId = DomainRules.Id(workflowVersionId, nameof(workflowVersionId));
        FromState = DomainRules.Text(fromState, nameof(fromState));
        ToState = DomainRules.Text(toState, nameof(toState));
        ConditionId = DomainRules.Id(conditionId, nameof(conditionId));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
    }

    public Guid WorkflowVersionId { get; }
    public string FromState { get; }
    public string ToState { get; }
    public Guid ConditionId { get; }
    public int Priority { get; }
}
