namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityDeviation model. References require application validation.</summary>
public sealed class QualityDeviation : OrganizationEntity
{
    public QualityDeviation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionResultId,
        string deviationCode,
        decimal observedValue,
        decimal expectedValue,
        string severity)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionResultId = DomainRules.Id(inspectionResultId, nameof(inspectionResultId));
        DeviationCode = DomainRules.Text(deviationCode, nameof(deviationCode));
        ObservedValue = DomainRules.Nonnegative(observedValue, nameof(observedValue));
        ExpectedValue = DomainRules.Nonnegative(expectedValue, nameof(expectedValue));
        Severity = DomainRules.Text(severity, nameof(severity));
    }

    public Guid InspectionResultId { get; }
    public string DeviationCode { get; }
    public decimal ObservedValue { get; }
    public decimal ExpectedValue { get; }
    public string Severity { get; }
}
