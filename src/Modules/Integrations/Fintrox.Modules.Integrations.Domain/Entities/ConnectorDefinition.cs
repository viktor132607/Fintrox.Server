namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ConnectorDefinition domain record; references checked in application layer.</summary>
public sealed class ConnectorDefinition : OrganizationEntity
{
    public ConnectorDefinition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string connectorCode,
        string name,
        string publisher,
        string connectorType,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorCode = DomainRules.Text(connectorCode, nameof(connectorCode));
        Name = DomainRules.Text(name, nameof(name));
        Publisher = DomainRules.Text(publisher, nameof(publisher));
        ConnectorType = DomainRules.Text(connectorType, nameof(connectorType));
        IsActive = isActive;
    }

    public string ConnectorCode { get; }
    public string Name { get; }
    public string Publisher { get; }
    public string ConnectorType { get; }
    public bool IsActive { get; }
}
