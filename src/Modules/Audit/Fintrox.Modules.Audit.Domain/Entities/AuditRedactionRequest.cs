namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditRedactionRequest record; cross-module references require application authorization.</summary>
public sealed class AuditRedactionRequest : OrganizationEntity
{
    public AuditRedactionRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid recordId,
        Guid requestedByUserId,
        string reason,
        DateTimeOffset requestedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        RecordId = DomainRules.Id(recordId, nameof(recordId));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        Reason = DomainRules.Text(reason, nameof(reason));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid RecordId { get; }
    public Guid RequestedByUserId { get; }
    public string Reason { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string Status { get; }
}
