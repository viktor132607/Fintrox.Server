namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditActorSnapshot record; cross-module references require application authorization.</summary>
public sealed class AuditActorSnapshot : OrganizationEntity
{
    public AuditActorSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid actorId,
        string displayName,
        string actorType,
        DateTimeOffset capturedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ActorId = DomainRules.Id(actorId, nameof(actorId));
        DisplayName = DomainRules.Text(displayName, nameof(displayName));
        ActorType = DomainRules.Text(actorType, nameof(actorType));
        CapturedAtUtc = DomainRules.Instant(capturedAtUtc, nameof(capturedAtUtc));
    }

    public Guid ActorId { get; }
    public string DisplayName { get; }
    public string ActorType { get; }
    public DateTimeOffset CapturedAtUtc { get; }
}
