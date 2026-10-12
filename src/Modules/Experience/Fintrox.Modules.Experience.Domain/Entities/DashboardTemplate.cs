namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned DashboardTemplate domain model.</summary>
public sealed class DashboardTemplate : OrganizationEntity
{
    public DashboardTemplate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string experienceLevel,
        bool isDefault)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        ExperienceLevel = DomainRules.Text(experienceLevel, nameof(experienceLevel));
        IsDefault = isDefault;
    }

    public string Code { get; }
    public string Name { get; }
    public string ExperienceLevel { get; }
    public bool IsDefault { get; }
}
