namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentImportItem. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentImportItem : OrganizationEntity
{
    public DocumentImportItem(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid importBatchId,
        string externalReference,
        Guid storedDocumentId,
        string importStatus)
        : base(id, organizationId, createdAtUtc)
    {
        ImportBatchId = DomainRules.Id(importBatchId, nameof(importBatchId));
        ExternalReference = DomainRules.Text(externalReference, nameof(externalReference));
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        ImportStatus = DomainRules.Text(importStatus, nameof(importStatus));
    }

    public Guid ImportBatchId { get; }
    public string ExternalReference { get; }
    public Guid StoredDocumentId { get; }
    public string ImportStatus { get; }
}
