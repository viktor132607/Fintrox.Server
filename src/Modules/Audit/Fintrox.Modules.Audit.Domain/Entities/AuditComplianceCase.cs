namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditComplianceCase record; cross-module references require application authorization.</summary>
public sealed class AuditComplianceCase : OrganizationEntity
{
    public AuditComplianceCase(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string caseNumber,
        string subject,
        DateTimeOffset openedAtUtc,
        Guid responsibleUserId,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        CaseNumber = DomainRules.Text(caseNumber, nameof(caseNumber));
        Subject = DomainRules.Text(subject, nameof(subject));
        OpenedAtUtc = DomainRules.Instant(openedAtUtc, nameof(openedAtUtc));
        ResponsibleUserId = DomainRules.Id(responsibleUserId, nameof(responsibleUserId));
        Status = DomainRules.Text(status, nameof(status));
    }

    public string CaseNumber { get; }
    public string Subject { get; }
    public DateTimeOffset OpenedAtUtc { get; }
    public Guid ResponsibleUserId { get; }
    public string Status { get; }
}
