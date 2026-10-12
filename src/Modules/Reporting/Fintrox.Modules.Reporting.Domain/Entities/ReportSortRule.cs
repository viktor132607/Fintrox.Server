namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportSortRule model. References require application validation.</summary>
public sealed class ReportSortRule : OrganizationEntity
{
    public ReportSortRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        string fieldCode,
        string direction,
        int priority)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        FieldCode = DomainRules.Text(fieldCode, nameof(fieldCode));
        Direction = DomainRules.Text(direction, nameof(direction));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
    }

    public Guid ReportDefinitionId { get; }
    public string FieldCode { get; }
    public string Direction { get; }
    public int Priority { get; }
}
