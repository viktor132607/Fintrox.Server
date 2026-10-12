namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationSecretRotation domain record; references checked in application layer.</summary>
public sealed class IntegrationSecretRotation : OrganizationEntity
{
    public IntegrationSecretRotation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorCredentialId,
        DateTimeOffset rotatedAtUtc,
        Guid rotatedByUserId,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorCredentialId = DomainRules.Id(connectorCredentialId, nameof(connectorCredentialId));
        RotatedAtUtc = DomainRules.Instant(rotatedAtUtc, nameof(rotatedAtUtc));
        RotatedByUserId = DomainRules.Id(rotatedByUserId, nameof(rotatedByUserId));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid ConnectorCredentialId { get; }
    public DateTimeOffset RotatedAtUtc { get; }
    public Guid RotatedByUserId { get; }
    public string Reason { get; }
}
