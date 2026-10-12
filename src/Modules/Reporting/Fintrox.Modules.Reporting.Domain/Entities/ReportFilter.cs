namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportFilter model. References require application validation.</summary>
public sealed class ReportFilter : OrganizationEntity
{
    public ReportFilter(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        string fieldCode,
        string filterOperator,
        string value)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        FieldCode = DomainRules.Text(fieldCode, nameof(fieldCode));
        FilterOperator = DomainRules.Text(filterOperator, nameof(filterOperator));
        Value = DomainRules.Text(value, nameof(value));
    }

    public Guid ReportDefinitionId { get; }
    public string FieldCode { get; }
    public string FilterOperator { get; }
    public string Value { get; }
}
