namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned ExperienceConfigurationVersion domain model.</summary>
public sealed class ExperienceConfigurationVersion : OrganizationEntity
{
    public ExperienceConfigurationVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid experienceProfileId,
        int versionNumber,
        string configurationJson,
        DateTimeOffset publishedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ExperienceProfileId = DomainRules.Id(experienceProfileId, nameof(experienceProfileId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        ConfigurationJson = DomainRules.Json(configurationJson, nameof(configurationJson));
        PublishedAtUtc = DomainRules.Instant(publishedAtUtc, nameof(publishedAtUtc));
    }

    public Guid ExperienceProfileId { get; }
    public int VersionNumber { get; }
    public string ConfigurationJson { get; }
    public DateTimeOffset PublishedAtUtc { get; }
}
