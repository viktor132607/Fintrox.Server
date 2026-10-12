namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowTaskDependency; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowTaskDependency : OrganizationEntity
{
    public WorkflowTaskDependency(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid predecessorTaskId,
        Guid successorTaskId,
        string dependencyType,
        bool isBlocking)
        : base(id, organizationId, createdAtUtc)
    {
        PredecessorTaskId = DomainRules.Id(predecessorTaskId, nameof(predecessorTaskId));
        SuccessorTaskId = DomainRules.Id(successorTaskId, nameof(successorTaskId));
        DependencyType = DomainRules.Text(dependencyType, nameof(dependencyType));
        IsBlocking = isBlocking;
    }

    public Guid PredecessorTaskId { get; }
    public Guid SuccessorTaskId { get; }
    public string DependencyType { get; }
    public bool IsBlocking { get; }
}
