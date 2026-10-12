namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned LayoutTemplateVersion domain model.</summary>
public sealed class LayoutTemplateVersion : OrganizationEntity
{
    public LayoutTemplateVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid layoutTemplateId,
        int versionNumber,
        string layoutJson,
        DateTimeOffset publishedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        LayoutTemplateId = DomainRules.Id(layoutTemplateId, nameof(layoutTemplateId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        LayoutJson = DomainRules.Json(layoutJson, nameof(layoutJson));
        PublishedAtUtc = DomainRules.Instant(publishedAtUtc, nameof(publishedAtUtc));
    }

    public Guid LayoutTemplateId { get; }
    public int VersionNumber { get; }
    public string LayoutJson { get; }
    public DateTimeOffset PublishedAtUtc { get; }
}
