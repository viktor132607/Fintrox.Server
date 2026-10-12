namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ConnectorCredential domain record; references checked in application layer.</summary>
public sealed class ConnectorCredential : OrganizationEntity
{
    public ConnectorCredential(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string secretReference,
        DateTimeOffset expiresAtUtc,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        SecretReference = DomainRules.Text(secretReference, nameof(secretReference));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
        IsActive = isActive;
    }

    public Guid ConnectorInstallationId { get; }
    public string SecretReference { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public bool IsActive { get; }
}
