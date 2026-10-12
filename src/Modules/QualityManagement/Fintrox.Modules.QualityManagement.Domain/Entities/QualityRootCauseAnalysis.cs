namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityRootCauseAnalysis model. References require application validation.</summary>
public sealed class QualityRootCauseAnalysis : OrganizationEntity
{
    public QualityRootCauseAnalysis(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid nonconformityId,
        string analysisMethod,
        string rootCause,
        Guid analyzedByUserId,
        DateOnly analyzedOn)
        : base(id, organizationId, createdAtUtc)
    {
        NonconformityId = DomainRules.Id(nonconformityId, nameof(nonconformityId));
        AnalysisMethod = DomainRules.Text(analysisMethod, nameof(analysisMethod));
        RootCause = DomainRules.Text(rootCause, nameof(rootCause));
        AnalyzedByUserId = DomainRules.Id(analyzedByUserId, nameof(analyzedByUserId));
        AnalyzedOn = DomainRules.Date(analyzedOn, nameof(analyzedOn));
    }

    public Guid NonconformityId { get; }
    public string AnalysisMethod { get; }
    public string RootCause { get; }
    public Guid AnalyzedByUserId { get; }
    public DateOnly AnalyzedOn { get; }
}
