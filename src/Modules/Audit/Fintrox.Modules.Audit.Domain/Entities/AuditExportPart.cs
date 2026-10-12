namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditExportPart record; cross-module references require application authorization.</summary>
public sealed class AuditExportPart : OrganizationEntity
{
    public AuditExportPart(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid exportRequestId,
        int partNumber,
        Guid documentId,
        int recordCount,
        string sha256)
        : base(id, organizationId, createdAtUtc)
    {
        ExportRequestId = DomainRules.Id(exportRequestId, nameof(exportRequestId));
        PartNumber = DomainRules.PositiveInt(partNumber, nameof(partNumber));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        RecordCount = DomainRules.NonnegativeInt(recordCount, nameof(recordCount));
        Sha256 = DomainRules.Hash(sha256, nameof(sha256));
    }

    public Guid ExportRequestId { get; }
    public int PartNumber { get; }
    public Guid DocumentId { get; }
    public int RecordCount { get; }
    public string Sha256 { get; }
}
