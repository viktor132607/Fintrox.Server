namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned LeadScore record. Domain invariants enforced at construction.</summary>
public sealed class LeadScore : OrganizationEntity
{
    public LeadScore(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid leadId,
        int score,
        string modelVersion,
        DateTimeOffset calculatedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        LeadId = DomainRules.Id(leadId, nameof(leadId));
        Score = DomainRules.NonnegativeInt(score, nameof(score));
        ModelVersion = DomainRules.Text(modelVersion, nameof(modelVersion));
        CalculatedAtUtc = DomainRules.Instant(calculatedAtUtc, nameof(calculatedAtUtc));
    }

    public Guid LeadId { get; }
    public int Score { get; }
    public string ModelVersion { get; }
    public DateTimeOffset CalculatedAtUtc { get; }
}
