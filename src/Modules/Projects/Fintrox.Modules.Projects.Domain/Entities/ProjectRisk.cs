namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectRisk; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectRisk : OrganizationEntity
{
    public ProjectRisk(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        string riskCode,
        string impact,
        string likelihood,
        string mitigation)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        RiskCode = DomainRules.Text(riskCode, nameof(riskCode));
        Impact = DomainRules.Text(impact, nameof(impact));
        Likelihood = DomainRules.Text(likelihood, nameof(likelihood));
        Mitigation = DomainRules.Text(mitigation, nameof(mitigation));
    }

    public Guid ProjectId { get; }
    public string RiskCode { get; }
    public string Impact { get; }
    public string Likelihood { get; }
    public string Mitigation { get; }
}
