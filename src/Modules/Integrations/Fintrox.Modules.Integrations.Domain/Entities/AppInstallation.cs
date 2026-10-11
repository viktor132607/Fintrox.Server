namespace Fintrox.Modules.Integrations.Domain.Entities;

public sealed class AppInstallation : OrganizationEntity
{
    public AppInstallation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid appId,
        Guid appVersionId)
        : base(id, organizationId, createdAtUtc)
    {
        AppId = DomainRules.Id(appId, nameof(appId));
        AppVersionId = DomainRules.Id(appVersionId, nameof(appVersionId));
    }

    public Guid AppId { get; }
    public Guid AppVersionId { get; }
}
