namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentImportBatch. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentImportBatch : OrganizationEntity
{
    public DocumentImportBatch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string sourceSystem,
        Guid importedByUserId,
        DateTimeOffset startedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SourceSystem = DomainRules.Text(sourceSystem, nameof(sourceSystem));
        ImportedByUserId = DomainRules.Id(importedByUserId, nameof(importedByUserId));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public string SourceSystem { get; }
    public Guid ImportedByUserId { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public string Status { get; }
}
