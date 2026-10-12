namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxAuditEvidence; tenant scope and input fields validated at creation.</summary>
public sealed class TaxAuditEvidence : OrganizationEntity
{
    public TaxAuditEvidence(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceDocumentId,
        Guid jurisdictionId,
        Guid evidenceDocumentId,
        DateOnly collectedOn)
        : base(id, organizationId, createdAtUtc)
    {
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        JurisdictionId = DomainRules.Id(jurisdictionId, nameof(jurisdictionId));
        EvidenceDocumentId = DomainRules.Id(evidenceDocumentId, nameof(evidenceDocumentId));
        CollectedOn = DomainRules.Date(collectedOn, nameof(collectedOn));
    }

    public Guid SourceDocumentId { get; }
    public Guid JurisdictionId { get; }
    public Guid EvidenceDocumentId { get; }
    public DateOnly CollectedOn { get; }
}
