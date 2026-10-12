namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceCompetency model; no cross-module navigations.</summary>
public sealed class ResourceCompetency : OrganizationEntity
{
    public ResourceCompetency(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        string competencyCode,
        int proficiencyLevel,
        DateOnly assessedOn)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        CompetencyCode = DomainRules.Text(competencyCode, nameof(competencyCode));
        ProficiencyLevel = DomainRules.PositiveInt(proficiencyLevel, nameof(proficiencyLevel));
        AssessedOn = DomainRules.Date(assessedOn, nameof(assessedOn));
    }

    public Guid ResourceId { get; }
    public string CompetencyCode { get; }
    public int ProficiencyLevel { get; }
    public DateOnly AssessedOn { get; }
}
