namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditAccessRecord record; cross-module references require application authorization.</summary>
public sealed class AuditAccessRecord : OrganizationEntity
{
    public AuditAccessRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid actorId,
        string resourceType,
        Guid resourceId,
        string accessType,
        DateTimeOffset accessedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ActorId = DomainRules.Id(actorId, nameof(actorId));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        AccessType = DomainRules.Text(accessType, nameof(accessType));
        AccessedAtUtc = DomainRules.Instant(accessedAtUtc, nameof(accessedAtUtc));
    }

    public Guid ActorId { get; }
    public string ResourceType { get; }
    public Guid ResourceId { get; }
    public string AccessType { get; }
    public DateTimeOffset AccessedAtUtc { get; }
}
