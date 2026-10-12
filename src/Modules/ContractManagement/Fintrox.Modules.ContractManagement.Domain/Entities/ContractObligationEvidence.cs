namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractObligationEvidence; external references validated by application layer.</summary>
public sealed class ContractObligationEvidence : OrganizationEntity
{
    public ContractObligationEvidence(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractObligationId,
        Guid documentId,
        Guid recordedByUserId,
        DateTimeOffset recordedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ContractObligationId = DomainRules.Id(contractObligationId, nameof(contractObligationId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        RecordedByUserId = DomainRules.Id(recordedByUserId, nameof(recordedByUserId));
        RecordedAtUtc = DomainRules.Instant(recordedAtUtc, nameof(recordedAtUtc));
    }

    public Guid ContractObligationId { get; }
    public Guid DocumentId { get; }
    public Guid RecordedByUserId { get; }
    public DateTimeOffset RecordedAtUtc { get; }
}
