namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportCalculatedField model. References require application validation.</summary>
public sealed class ReportCalculatedField : OrganizationEntity
{
    public ReportCalculatedField(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        string fieldCode,
        string formula,
        string dataType)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        FieldCode = DomainRules.Text(fieldCode, nameof(fieldCode));
        Formula = DomainRules.Text(formula, nameof(formula));
        DataType = DomainRules.Text(dataType, nameof(dataType));
    }

    public Guid ReportDefinitionId { get; }
    public string FieldCode { get; }
    public string Formula { get; }
    public string DataType { get; }
}
