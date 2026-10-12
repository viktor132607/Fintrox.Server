namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractSignature; external references validated by application layer.</summary>
public sealed class ContractSignature : OrganizationEntity
{
    public ContractSignature(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractVersionId,
        Guid signatoryId,
        DateTimeOffset signedAtUtc,
        string signatureMethod,
        bool isVerified)
        : base(id, organizationId, createdAtUtc)
    {
        ContractVersionId = DomainRules.Id(contractVersionId, nameof(contractVersionId));
        SignatoryId = DomainRules.Id(signatoryId, nameof(signatoryId));
        SignedAtUtc = DomainRules.Instant(signedAtUtc, nameof(signedAtUtc));
        SignatureMethod = DomainRules.Text(signatureMethod, nameof(signatureMethod));
        IsVerified = isVerified;
    }

    public Guid ContractVersionId { get; }
    public Guid SignatoryId { get; }
    public DateTimeOffset SignedAtUtc { get; }
    public string SignatureMethod { get; }
    public bool IsVerified { get; }
}
