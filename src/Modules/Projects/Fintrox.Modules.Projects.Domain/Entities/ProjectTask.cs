namespace Fintrox.Modules.Projects.Domain.Entities;

public sealed class ProjectTask : OrganizationEntity
{
    public ProjectTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        string name,
        DateOnly dueOn)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        Name = DomainRules.Text(name, nameof(name));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
    }

    public Guid ProjectId { get; }
    public string Name { get; }
    public DateOnly DueOn { get; }
}
