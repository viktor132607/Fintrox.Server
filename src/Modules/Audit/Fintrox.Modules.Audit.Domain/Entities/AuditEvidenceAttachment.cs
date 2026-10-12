namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditEvidenceAttachment record; cross-module references require application authorization.</summary>
public sealed class AuditEvidenceAttachment : OrganizationEntity
{
    public AuditEvidenceAttachment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid caseId,
        Guid documentId,
        string description,
        DateTimeOffset collectedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        CaseId = DomainRules.Id(caseId, nameof(caseId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        Description = DomainRules.Text(description, nameof(description));
        CollectedAtUtc = DomainRules.Instant(collectedAtUtc, nameof(collectedAtUtc));
    }

    public Guid CaseId { get; }
    public Guid DocumentId { get; }
    public string Description { get; }
    public DateTimeOffset CollectedAtUtc { get; }
}
