namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowTrigger; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowTrigger : OrganizationEntity
{
    public WorkflowTrigger(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowVersionId,
        string triggerType,
        string eventCode,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowVersionId = DomainRules.Id(workflowVersionId, nameof(workflowVersionId));
        TriggerType = DomainRules.Text(triggerType, nameof(triggerType));
        EventCode = DomainRules.Text(eventCode, nameof(eventCode));
        IsEnabled = isEnabled;
    }

    public Guid WorkflowVersionId { get; }
    public string TriggerType { get; }
    public string EventCode { get; }
    public bool IsEnabled { get; }
}
