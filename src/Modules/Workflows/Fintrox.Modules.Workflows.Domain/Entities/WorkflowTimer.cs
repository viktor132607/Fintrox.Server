namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowTimer; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowTimer : OrganizationEntity
{
    public WorkflowTimer(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowInstanceId,
        string timerCode,
        DateTimeOffset fireAtUtc,
        bool isCancelled)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowInstanceId = DomainRules.Id(workflowInstanceId, nameof(workflowInstanceId));
        TimerCode = DomainRules.Text(timerCode, nameof(timerCode));
        FireAtUtc = DomainRules.Instant(fireAtUtc, nameof(fireAtUtc));
        IsCancelled = isCancelled;
    }

    public Guid WorkflowInstanceId { get; }
    public string TimerCode { get; }
    public DateTimeOffset FireAtUtc { get; }
    public bool IsCancelled { get; }
}
