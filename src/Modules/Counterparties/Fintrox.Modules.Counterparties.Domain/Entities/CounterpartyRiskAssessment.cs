namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyRiskAssessment. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyRiskAssessment : OrganizationEntity
{
    public CounterpartyRiskAssessment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string riskCategory,
        int riskScore,
        DateOnly assessedOn,
        Guid reviewerUserId)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        RiskCategory = DomainRules.Text(riskCategory, nameof(riskCategory));
        RiskScore = DomainRules.NonnegativeInt(riskScore, nameof(riskScore));
        AssessedOn = DomainRules.Date(assessedOn, nameof(assessedOn));
        ReviewerUserId = DomainRules.Id(reviewerUserId, nameof(reviewerUserId));
    }

    public Guid CounterpartyId { get; }
    public string RiskCategory { get; }
    public int RiskScore { get; }
    public DateOnly AssessedOn { get; }
    public Guid ReviewerUserId { get; }
}
