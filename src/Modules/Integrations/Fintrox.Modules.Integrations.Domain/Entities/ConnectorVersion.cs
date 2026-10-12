namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ConnectorVersion domain record; references checked in application layer.</summary>
public sealed class ConnectorVersion : OrganizationEntity
{
    public ConnectorVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorDefinitionId,
        string versionNumber,
        DateOnly releasedOn,
        string configurationSchemaJson)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorDefinitionId = DomainRules.Id(connectorDefinitionId, nameof(connectorDefinitionId));
        VersionNumber = DomainRules.Text(versionNumber, nameof(versionNumber));
        ReleasedOn = DomainRules.Date(releasedOn, nameof(releasedOn));
        ConfigurationSchemaJson = DomainRules.Json(configurationSchemaJson, nameof(configurationSchemaJson));
    }

    public Guid ConnectorDefinitionId { get; }
    public string VersionNumber { get; }
    public DateOnly ReleasedOn { get; }
    public string ConfigurationSchemaJson { get; }
}
