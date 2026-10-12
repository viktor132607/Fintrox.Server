namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowAutomationRun; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowAutomationRun : OrganizationEntity
{
    public WorkflowAutomationRun(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowVersionId,
        DateTimeOffset triggeredAtUtc,
        Guid triggerReferenceId,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowVersionId = DomainRules.Id(workflowVersionId, nameof(workflowVersionId));
        TriggeredAtUtc = DomainRules.Instant(triggeredAtUtc, nameof(triggeredAtUtc));
        TriggerReferenceId = DomainRules.Id(triggerReferenceId, nameof(triggerReferenceId));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WorkflowVersionId { get; }
    public DateTimeOffset TriggeredAtUtc { get; }
    public Guid TriggerReferenceId { get; }
    public string Status { get; }
}
