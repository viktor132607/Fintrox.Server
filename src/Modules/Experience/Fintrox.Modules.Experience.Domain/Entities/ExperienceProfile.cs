namespace Fintrox.Modules.Experience.Domain.Entities;

public sealed class ExperienceProfile : OrganizationEntity
{
    public ExperienceProfile(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        ExperienceMode mode)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        Mode = DomainRules.Defined(mode, nameof(mode));
    }

    public string Name { get; }
    public ExperienceMode Mode { get; }
}
