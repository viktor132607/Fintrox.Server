namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ConnectorMappingRule domain record; references checked in application layer.</summary>
public sealed class ConnectorMappingRule : OrganizationEntity
{
    public ConnectorMappingRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string localEntityType,
        string externalEntityType,
        string direction,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        LocalEntityType = DomainRules.Text(localEntityType, nameof(localEntityType));
        ExternalEntityType = DomainRules.Text(externalEntityType, nameof(externalEntityType));
        Direction = DomainRules.Text(direction, nameof(direction));
        IsEnabled = isEnabled;
    }

    public Guid ConnectorInstallationId { get; }
    public string LocalEntityType { get; }
    public string ExternalEntityType { get; }
    public string Direction { get; }
    public bool IsEnabled { get; }
}
