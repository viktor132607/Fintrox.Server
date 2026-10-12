namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectPhase; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectPhase : OrganizationEntity
{
    public ProjectPhase(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        string phaseCode,
        string name,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        PhaseCode = DomainRules.Text(phaseCode, nameof(phaseCode));
        Name = DomainRules.Text(name, nameof(name));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid ProjectId { get; }
    public string PhaseCode { get; }
    public string Name { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
