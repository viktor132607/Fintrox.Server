namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportParameter model. References require application validation.</summary>
public sealed class ReportParameter : OrganizationEntity
{
    public ReportParameter(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        string parameterCode,
        string dataType,
        string defaultValue,
        bool isRequired)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        ParameterCode = DomainRules.Text(parameterCode, nameof(parameterCode));
        DataType = DomainRules.Text(dataType, nameof(dataType));
        DefaultValue = DomainRules.Text(defaultValue, nameof(defaultValue));
        IsRequired = isRequired;
    }

    public Guid ReportDefinitionId { get; }
    public string ParameterCode { get; }
    public string DataType { get; }
    public string DefaultValue { get; }
    public bool IsRequired { get; }
}
