namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentRetentionSchedule. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentRetentionSchedule : OrganizationEntity
{
    public DocumentRetentionSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string documentType,
        int retentionDays,
        string legalBasis,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        DocumentType = DomainRules.Text(documentType, nameof(documentType));
        RetentionDays = DomainRules.PositiveInt(retentionDays, nameof(retentionDays));
        LegalBasis = DomainRules.Text(legalBasis, nameof(legalBasis));
        IsActive = isActive;
    }

    public string DocumentType { get; }
    public int RetentionDays { get; }
    public string LegalBasis { get; }
    public bool IsActive { get; }
}
