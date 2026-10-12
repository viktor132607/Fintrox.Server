namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditAlert record; cross-module references require application authorization.</summary>
public sealed class AuditAlert : OrganizationEntity
{
    public AuditAlert(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid eventRuleId,
        Guid auditEventId,
        string severity,
        DateTimeOffset detectedAtUtc,
        bool isAcknowledged)
        : base(id, organizationId, createdAtUtc)
    {
        EventRuleId = DomainRules.Id(eventRuleId, nameof(eventRuleId));
        AuditEventId = DomainRules.Id(auditEventId, nameof(auditEventId));
        Severity = DomainRules.Text(severity, nameof(severity));
        DetectedAtUtc = DomainRules.Instant(detectedAtUtc, nameof(detectedAtUtc));
        IsAcknowledged = isAcknowledged;
    }

    public Guid EventRuleId { get; }
    public Guid AuditEventId { get; }
    public string Severity { get; }
    public DateTimeOffset DetectedAtUtc { get; }
    public bool IsAcknowledged { get; }
}
