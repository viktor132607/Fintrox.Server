namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentUploadPart. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentUploadPart : OrganizationEntity
{
    public DocumentUploadPart(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid uploadSessionId,
        int partNumber,
        long partSizeBytes,
        string sha256)
        : base(id, organizationId, createdAtUtc)
    {
        UploadSessionId = DomainRules.Id(uploadSessionId, nameof(uploadSessionId));
        PartNumber = DomainRules.PositiveInt(partNumber, nameof(partNumber));
        PartSizeBytes = DomainRules.NonnegativeLong(partSizeBytes, nameof(partSizeBytes));
        Sha256 = DomainRules.Hash(sha256, nameof(sha256));
    }

    public Guid UploadSessionId { get; }
    public int PartNumber { get; }
    public long PartSizeBytes { get; }
    public string Sha256 { get; }
}
