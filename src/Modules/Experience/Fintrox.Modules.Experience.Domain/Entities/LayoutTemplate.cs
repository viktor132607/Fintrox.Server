namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned LayoutTemplate domain model.</summary>
public sealed class LayoutTemplate : OrganizationEntity
{
    public LayoutTemplate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string experienceLevel,
        bool isSystemDefault)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        ExperienceLevel = DomainRules.Text(experienceLevel, nameof(experienceLevel));
        IsSystemDefault = isSystemDefault;
    }

    public string Code { get; }
    public string Name { get; }
    public string ExperienceLevel { get; }
    public bool IsSystemDefault { get; }
}
