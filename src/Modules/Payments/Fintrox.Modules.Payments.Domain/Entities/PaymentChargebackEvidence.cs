namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentChargebackEvidence; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentChargebackEvidence : OrganizationEntity
{
    public PaymentChargebackEvidence(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid chargebackId,
        Guid documentId,
        DateTimeOffset submittedAtUtc,
        string evidenceType)
        : base(id, organizationId, createdAtUtc)
    {
        ChargebackId = DomainRules.Id(chargebackId, nameof(chargebackId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        SubmittedAtUtc = DomainRules.Instant(submittedAtUtc, nameof(submittedAtUtc));
        EvidenceType = DomainRules.Text(evidenceType, nameof(evidenceType));
    }

    public Guid ChargebackId { get; }
    public Guid DocumentId { get; }
    public DateTimeOffset SubmittedAtUtc { get; }
    public string EvidenceType { get; }
}
