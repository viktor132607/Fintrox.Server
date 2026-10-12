namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectDependency; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectDependency : OrganizationEntity
{
    public ProjectDependency(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid predecessorTaskId,
        Guid successorTaskId,
        string dependencyType,
        int lagDays)
        : base(id, organizationId, createdAtUtc)
    {
        PredecessorTaskId = DomainRules.Id(predecessorTaskId, nameof(predecessorTaskId));
        SuccessorTaskId = DomainRules.Id(successorTaskId, nameof(successorTaskId));
        DependencyType = DomainRules.Text(dependencyType, nameof(dependencyType));
        LagDays = DomainRules.NonnegativeInt(lagDays, nameof(lagDays));
    }

    public Guid PredecessorTaskId { get; }
    public Guid SuccessorTaskId { get; }
    public string DependencyType { get; }
    public int LagDays { get; }
}
