namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned LeadQualification record. Domain invariants enforced at construction.</summary>
public sealed class LeadQualification : OrganizationEntity
{
    public LeadQualification(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid leadId,
        string criterion,
        string assessment,
        int score,
        Guid assessedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        LeadId = DomainRules.Id(leadId, nameof(leadId));
        Criterion = DomainRules.Text(criterion, nameof(criterion));
        Assessment = DomainRules.Text(assessment, nameof(assessment));
        Score = DomainRules.NonnegativeInt(score, nameof(score));
        AssessedByUserId = DomainRules.Id(assessedByUserId, nameof(assessedByUserId));
    }

    public Guid LeadId { get; }
    public string Criterion { get; }
    public string Assessment { get; }
    public int Score { get; }
    public Guid AssessedByUserId { get; }
}
