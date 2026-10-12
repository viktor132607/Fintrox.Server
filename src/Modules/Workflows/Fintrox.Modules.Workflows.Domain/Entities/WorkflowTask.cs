namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowTask; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowTask : OrganizationEntity
{
    public WorkflowTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowInstanceId,
        string taskCode,
        string subject,
        DateTimeOffset dueAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowInstanceId = DomainRules.Id(workflowInstanceId, nameof(workflowInstanceId));
        TaskCode = DomainRules.Text(taskCode, nameof(taskCode));
        Subject = DomainRules.Text(subject, nameof(subject));
        DueAtUtc = DomainRules.Instant(dueAtUtc, nameof(dueAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WorkflowInstanceId { get; }
    public string TaskCode { get; }
    public string Subject { get; }
    public DateTimeOffset DueAtUtc { get; }
    public string Status { get; }
}
