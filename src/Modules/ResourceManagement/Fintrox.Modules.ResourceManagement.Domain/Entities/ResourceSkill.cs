namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

public sealed class ResourceSkill : OrganizationEntity
{
    public ResourceSkill(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        string skillCode,
        int proficiencyLevel)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        SkillCode = DomainRules.Text(skillCode, nameof(skillCode));
        ProficiencyLevel = DomainRules.Level(proficiencyLevel, nameof(proficiencyLevel));
    }

    public Guid ResourceId { get; }
    public string SkillCode { get; }
    public int ProficiencyLevel { get; }
}
