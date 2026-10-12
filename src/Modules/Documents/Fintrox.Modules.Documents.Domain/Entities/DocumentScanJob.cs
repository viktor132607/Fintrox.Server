namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentScanJob. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentScanJob : OrganizationEntity
{
    public DocumentScanJob(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storedDocumentId,
        string scannerName,
        DateTimeOffset submittedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        ScannerName = DomainRules.Text(scannerName, nameof(scannerName));
        SubmittedAtUtc = DomainRules.Instant(submittedAtUtc, nameof(submittedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid StoredDocumentId { get; }
    public string ScannerName { get; }
    public DateTimeOffset SubmittedAtUtc { get; }
    public string Status { get; }
}
