namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowExecutionLog; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowExecutionLog : OrganizationEntity
{
    public WorkflowExecutionLog(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowInstanceId,
        string stepCode,
        string eventType,
        DateTimeOffset occurredAtUtc,
        string result)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowInstanceId = DomainRules.Id(workflowInstanceId, nameof(workflowInstanceId));
        StepCode = DomainRules.Text(stepCode, nameof(stepCode));
        EventType = DomainRules.Text(eventType, nameof(eventType));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        Result = DomainRules.Text(result, nameof(result));
    }

    public Guid WorkflowInstanceId { get; }
    public string StepCode { get; }
    public string EventType { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string Result { get; }
}
