namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractDocument; external references validated by application layer.</summary>
public sealed class ContractDocument : OrganizationEntity
{
    public ContractDocument(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractVersionId,
        Guid documentId,
        string documentType,
        DateTimeOffset attachedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ContractVersionId = DomainRules.Id(contractVersionId, nameof(contractVersionId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        DocumentType = DomainRules.Text(documentType, nameof(documentType));
        AttachedAtUtc = DomainRules.Instant(attachedAtUtc, nameof(attachedAtUtc));
    }

    public Guid ContractVersionId { get; }
    public Guid DocumentId { get; }
    public string DocumentType { get; }
    public DateTimeOffset AttachedAtUtc { get; }
}
