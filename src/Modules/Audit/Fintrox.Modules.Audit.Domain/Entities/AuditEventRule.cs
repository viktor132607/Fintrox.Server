namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditEventRule record; cross-module references require application authorization.</summary>
public sealed class AuditEventRule : OrganizationEntity
{
    public AuditEventRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string eventCategory,
        string severity,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        EventCategory = DomainRules.Text(eventCategory, nameof(eventCategory));
        Severity = DomainRules.Text(severity, nameof(severity));
        IsEnabled = isEnabled;
    }

    public string Code { get; }
    public string EventCategory { get; }
    public string Severity { get; }
    public bool IsEnabled { get; }
}
