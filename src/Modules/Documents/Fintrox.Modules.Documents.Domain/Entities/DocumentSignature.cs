namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentSignature. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentSignature : OrganizationEntity
{
    public DocumentSignature(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid signatureRequestId,
        DateTimeOffset signedAtUtc,
        string certificateReference,
        bool isVerified)
        : base(id, organizationId, createdAtUtc)
    {
        SignatureRequestId = DomainRules.Id(signatureRequestId, nameof(signatureRequestId));
        SignedAtUtc = DomainRules.Instant(signedAtUtc, nameof(signedAtUtc));
        CertificateReference = DomainRules.Text(certificateReference, nameof(certificateReference));
        IsVerified = isVerified;
    }

    public Guid SignatureRequestId { get; }
    public DateTimeOffset SignedAtUtc { get; }
    public string CertificateReference { get; }
    public bool IsVerified { get; }
}
