namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ExternalEventSchema domain record; references checked in application layer.</summary>
public sealed class ExternalEventSchema : OrganizationEntity
{
    public ExternalEventSchema(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorDefinitionId,
        string eventType,
        string schemaVersion,
        string schemaJson)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorDefinitionId = DomainRules.Id(connectorDefinitionId, nameof(connectorDefinitionId));
        EventType = DomainRules.Text(eventType, nameof(eventType));
        SchemaVersion = DomainRules.Text(schemaVersion, nameof(schemaVersion));
        SchemaJson = DomainRules.Json(schemaJson, nameof(schemaJson));
    }

    public Guid ConnectorDefinitionId { get; }
    public string EventType { get; }
    public string SchemaVersion { get; }
    public string SchemaJson { get; }
}
