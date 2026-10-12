namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned ExperienceAccessPolicy domain model.</summary>
public sealed class ExperienceAccessPolicy : OrganizationEntity
{
    public ExperienceAccessPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid profileId,
        string capabilityCode,
        string minimumLevel,
        bool isAllowed)
        : base(id, organizationId, createdAtUtc)
    {
        ProfileId = DomainRules.Id(profileId, nameof(profileId));
        CapabilityCode = DomainRules.Text(capabilityCode, nameof(capabilityCode));
        MinimumLevel = DomainRules.Text(minimumLevel, nameof(minimumLevel));
        IsAllowed = isAllowed;
    }

    public Guid ProfileId { get; }
    public string CapabilityCode { get; }
    public string MinimumLevel { get; }
    public bool IsAllowed { get; }
}
