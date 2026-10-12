namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowTaskAssignment; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowTaskAssignment : OrganizationEntity
{
    public WorkflowTaskAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowTaskId,
        Guid assignedUserId,
        DateTimeOffset assignedAtUtc,
        string assignmentType)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowTaskId = DomainRules.Id(workflowTaskId, nameof(workflowTaskId));
        AssignedUserId = DomainRules.Id(assignedUserId, nameof(assignedUserId));
        AssignedAtUtc = DomainRules.Instant(assignedAtUtc, nameof(assignedAtUtc));
        AssignmentType = DomainRules.Text(assignmentType, nameof(assignmentType));
    }

    public Guid WorkflowTaskId { get; }
    public Guid AssignedUserId { get; }
    public DateTimeOffset AssignedAtUtc { get; }
    public string AssignmentType { get; }
}
