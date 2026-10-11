namespace Fintrox.Modules.Audit.Domain.Entities;

public sealed class AuditExportRequest : OrganizationEntity
{
    public AuditExportRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid requestedByUserId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc)
        : base(id, organizationId, createdAtUtc)
    {
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        FromUtc = DomainRules.Instant(fromUtc, nameof(fromUtc));
        ToUtc = DomainRules.Instant(toUtc, nameof(toUtc));
        if (ToUtc < FromUtc) throw new ArgumentException("The end cannot precede the start.", nameof(toUtc));
    }

    public Guid RequestedByUserId { get; }
    public DateTimeOffset FromUtc { get; }
    public DateTimeOffset ToUtc { get; }
}
