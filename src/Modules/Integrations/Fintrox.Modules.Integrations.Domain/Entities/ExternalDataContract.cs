namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ExternalDataContract domain record; references checked in application layer.</summary>
public sealed class ExternalDataContract : OrganizationEntity
{
    public ExternalDataContract(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorDefinitionId,
        string contractCode,
        string schemaVersion,
        string schemaJson)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorDefinitionId = DomainRules.Id(connectorDefinitionId, nameof(connectorDefinitionId));
        ContractCode = DomainRules.Text(contractCode, nameof(contractCode));
        SchemaVersion = DomainRules.Text(schemaVersion, nameof(schemaVersion));
        SchemaJson = DomainRules.Json(schemaJson, nameof(schemaJson));
    }

    public Guid ConnectorDefinitionId { get; }
    public string ContractCode { get; }
    public string SchemaVersion { get; }
    public string SchemaJson { get; }
}
