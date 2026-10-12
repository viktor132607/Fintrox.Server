namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportRunParameter model. References require application validation.</summary>
public sealed class ReportRunParameter : OrganizationEntity
{
    public ReportRunParameter(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportRunId,
        string parameterCode,
        string value,
        bool isSensitive)
        : base(id, organizationId, createdAtUtc)
    {
        ReportRunId = DomainRules.Id(reportRunId, nameof(reportRunId));
        ParameterCode = DomainRules.Text(parameterCode, nameof(parameterCode));
        Value = DomainRules.Text(value, nameof(value));
        IsSensitive = isSensitive;
    }

    public Guid ReportRunId { get; }
    public string ParameterCode { get; }
    public string Value { get; }
    public bool IsSensitive { get; }
}
