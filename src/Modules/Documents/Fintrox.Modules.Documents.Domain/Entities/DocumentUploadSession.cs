namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentUploadSession. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentUploadSession : OrganizationEntity
{
    public DocumentUploadSession(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid ownerUserId,
        string filename,
        long expectedSizeBytes,
        DateTimeOffset startedAtUtc,
        DateTimeOffset expiresAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
        Filename = DomainRules.Text(filename, nameof(filename));
        ExpectedSizeBytes = DomainRules.NonnegativeLong(expectedSizeBytes, nameof(expectedSizeBytes));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
    }

    public Guid OwnerUserId { get; }
    public string Filename { get; }
    public long ExpectedSizeBytes { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
}
