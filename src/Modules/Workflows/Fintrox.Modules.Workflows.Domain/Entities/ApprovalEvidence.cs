namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned ApprovalEvidence; tenant scope and input fields validated at creation.</summary>
public sealed class ApprovalEvidence : OrganizationEntity
{
    public ApprovalEvidence(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid approvalDecisionId,
        Guid documentId,
        DateTimeOffset recordedAtUtc,
        string description)
        : base(id, organizationId, createdAtUtc)
    {
        ApprovalDecisionId = DomainRules.Id(approvalDecisionId, nameof(approvalDecisionId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        RecordedAtUtc = DomainRules.Instant(recordedAtUtc, nameof(recordedAtUtc));
        Description = DomainRules.Text(description, nameof(description));
    }

    public Guid ApprovalDecisionId { get; }
    public Guid DocumentId { get; }
    public DateTimeOffset RecordedAtUtc { get; }
    public string Description { get; }
}
