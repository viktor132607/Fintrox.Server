namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned DashboardSection domain model.</summary>
public sealed class DashboardSection : OrganizationEntity
{
    public DashboardSection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid dashboardTemplateId,
        string sectionCode,
        int position,
        string title)
        : base(id, organizationId, createdAtUtc)
    {
        DashboardTemplateId = DomainRules.Id(dashboardTemplateId, nameof(dashboardTemplateId));
        SectionCode = DomainRules.Text(sectionCode, nameof(sectionCode));
        Position = DomainRules.NonnegativeInt(position, nameof(position));
        Title = DomainRules.Text(title, nameof(title));
    }

    public Guid DashboardTemplateId { get; }
    public string SectionCode { get; }
    public int Position { get; }
    public string Title { get; }
}
