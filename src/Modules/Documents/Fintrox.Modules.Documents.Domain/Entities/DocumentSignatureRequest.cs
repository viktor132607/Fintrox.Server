namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentSignatureRequest. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentSignatureRequest : OrganizationEntity
{
    public DocumentSignatureRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storedDocumentId,
        Guid signatoryId,
        DateTimeOffset requestedAtUtc,
        DateOnly dueOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        SignatoryId = DomainRules.Id(signatoryId, nameof(signatoryId));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid StoredDocumentId { get; }
    public Guid SignatoryId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public DateOnly DueOn { get; }
    public string Status { get; }
}
