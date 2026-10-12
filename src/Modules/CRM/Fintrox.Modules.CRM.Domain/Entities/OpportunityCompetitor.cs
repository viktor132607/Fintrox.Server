namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned OpportunityCompetitor record. Domain invariants enforced at construction.</summary>
public sealed class OpportunityCompetitor : OrganizationEntity
{
    public OpportunityCompetitor(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid opportunityId,
        string competitorName,
        string threatLevel,
        string notes)
        : base(id, organizationId, createdAtUtc)
    {
        OpportunityId = DomainRules.Id(opportunityId, nameof(opportunityId));
        CompetitorName = DomainRules.Text(competitorName, nameof(competitorName));
        ThreatLevel = DomainRules.Text(threatLevel, nameof(threatLevel));
        Notes = DomainRules.Text(notes, nameof(notes));
    }

    public Guid OpportunityId { get; }
    public string CompetitorName { get; }
    public string ThreatLevel { get; }
    public string Notes { get; }
}
