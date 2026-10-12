namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationAuditRecord domain record; references checked in application layer.</summary>
public sealed class IntegrationAuditRecord : OrganizationEntity
{
    public IntegrationAuditRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        Guid actorId,
        string action,
        DateTimeOffset occurredAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        ActorId = DomainRules.Id(actorId, nameof(actorId));
        Action = DomainRules.Text(action, nameof(action));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
    }

    public Guid ConnectorInstallationId { get; }
    public Guid ActorId { get; }
    public string Action { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}
