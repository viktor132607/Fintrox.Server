namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationPermissionScope domain record; references checked in application layer.</summary>
public sealed class IntegrationPermissionScope : OrganizationEntity
{
    public IntegrationPermissionScope(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string scopeCode,
        string resourceType,
        bool isGranted)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        ScopeCode = DomainRules.Text(scopeCode, nameof(scopeCode));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        IsGranted = isGranted;
    }

    public Guid ConnectorInstallationId { get; }
    public string ScopeCode { get; }
    public string ResourceType { get; }
    public bool IsGranted { get; }
}
