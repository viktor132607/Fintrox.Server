namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ConnectorMappingField domain record; references checked in application layer.</summary>
public sealed class ConnectorMappingField : OrganizationEntity
{
    public ConnectorMappingField(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid mappingRuleId,
        string localProperty,
        string externalProperty,
        string transformType,
        bool isRequired)
        : base(id, organizationId, createdAtUtc)
    {
        MappingRuleId = DomainRules.Id(mappingRuleId, nameof(mappingRuleId));
        LocalProperty = DomainRules.Text(localProperty, nameof(localProperty));
        ExternalProperty = DomainRules.Text(externalProperty, nameof(externalProperty));
        TransformType = DomainRules.Text(transformType, nameof(transformType));
        IsRequired = isRequired;
    }

    public Guid MappingRuleId { get; }
    public string LocalProperty { get; }
    public string ExternalProperty { get; }
    public string TransformType { get; }
    public bool IsRequired { get; }
}
