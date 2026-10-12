namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportGrouping model. References require application validation.</summary>
public sealed class ReportGrouping : OrganizationEntity
{
    public ReportGrouping(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        string fieldCode,
        int groupingLevel,
        bool isCollapsed)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        FieldCode = DomainRules.Text(fieldCode, nameof(fieldCode));
        GroupingLevel = DomainRules.PositiveInt(groupingLevel, nameof(groupingLevel));
        IsCollapsed = isCollapsed;
    }

    public Guid ReportDefinitionId { get; }
    public string FieldCode { get; }
    public int GroupingLevel { get; }
    public bool IsCollapsed { get; }
}
