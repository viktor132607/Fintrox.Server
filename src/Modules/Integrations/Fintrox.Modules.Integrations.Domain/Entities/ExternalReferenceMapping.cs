namespace Fintrox.Modules.Integrations.Domain.Entities;

public sealed class ExternalReferenceMapping : OrganizationEntity
{
    public ExternalReferenceMapping(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid appInstallationId,
        string externalType,
        string externalId,
        string resourceType,
        Guid resourceId)
        : base(id, organizationId, createdAtUtc)
    {
        AppInstallationId = DomainRules.Id(appInstallationId, nameof(appInstallationId));
        ExternalType = DomainRules.Text(externalType, nameof(externalType));
        ExternalId = DomainRules.Text(externalId, nameof(externalId));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
    }

    public Guid AppInstallationId { get; }
    public string ExternalType { get; }
    public string ExternalId { get; }
    public string ResourceType { get; }
    public Guid ResourceId { get; }
}
