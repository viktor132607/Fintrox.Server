namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationCapabilityGrant domain record; references checked in application layer.</summary>
public sealed class IntegrationCapabilityGrant : OrganizationEntity
{
    public IntegrationCapabilityGrant(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string capabilityCode,
        Guid grantedByUserId,
        DateTimeOffset grantedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        CapabilityCode = DomainRules.Text(capabilityCode, nameof(capabilityCode));
        GrantedByUserId = DomainRules.Id(grantedByUserId, nameof(grantedByUserId));
        GrantedAtUtc = DomainRules.Instant(grantedAtUtc, nameof(grantedAtUtc));
    }

    public Guid ConnectorInstallationId { get; }
    public string CapabilityCode { get; }
    public Guid GrantedByUserId { get; }
    public DateTimeOffset GrantedAtUtc { get; }
}
