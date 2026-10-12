namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportSection model. References require application validation.</summary>
public sealed class ReportSection : OrganizationEntity
{
    public ReportSection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportLayoutId,
        string sectionCode,
        string title,
        int position)
        : base(id, organizationId, createdAtUtc)
    {
        ReportLayoutId = DomainRules.Id(reportLayoutId, nameof(reportLayoutId));
        SectionCode = DomainRules.Text(sectionCode, nameof(sectionCode));
        Title = DomainRules.Text(title, nameof(title));
        Position = DomainRules.NonnegativeInt(position, nameof(position));
    }

    public Guid ReportLayoutId { get; }
    public string SectionCode { get; }
    public string Title { get; }
    public int Position { get; }
}
