namespace Fintrox.Modules.Integrations.Domain.Entities;

public sealed class AppVersion : OrganizationEntity
{
    public AppVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid appId,
        string version,
        string manifestJson)
        : base(id, organizationId, createdAtUtc)
    {
        AppId = DomainRules.Id(appId, nameof(appId));
        Version = DomainRules.Text(version, nameof(version));
        ManifestJson = DomainRules.Json(manifestJson, nameof(manifestJson));
    }

    public Guid AppId { get; }
    public string Version { get; }
    public string ManifestJson { get; }
}
