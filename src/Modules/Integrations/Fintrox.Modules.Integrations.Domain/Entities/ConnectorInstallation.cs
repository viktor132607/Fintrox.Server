namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ConnectorInstallation domain record; references checked in application layer.</summary>
public sealed class ConnectorInstallation : OrganizationEntity
{
    public ConnectorInstallation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorDefinitionId,
        Guid versionId,
        Guid installedByUserId,
        DateTimeOffset installedAtUtc,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorDefinitionId = DomainRules.Id(connectorDefinitionId, nameof(connectorDefinitionId));
        VersionId = DomainRules.Id(versionId, nameof(versionId));
        InstalledByUserId = DomainRules.Id(installedByUserId, nameof(installedByUserId));
        InstalledAtUtc = DomainRules.Instant(installedAtUtc, nameof(installedAtUtc));
        IsEnabled = isEnabled;
    }

    public Guid ConnectorDefinitionId { get; }
    public Guid VersionId { get; }
    public Guid InstalledByUserId { get; }
    public DateTimeOffset InstalledAtUtc { get; }
    public bool IsEnabled { get; }
}
