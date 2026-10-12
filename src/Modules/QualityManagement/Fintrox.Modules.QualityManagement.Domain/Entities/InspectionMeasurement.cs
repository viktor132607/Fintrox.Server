namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped InspectionMeasurement model. References require application validation.</summary>
public sealed class InspectionMeasurement : OrganizationEntity
{
    public InspectionMeasurement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionSampleId,
        Guid criterionId,
        decimal measuredValue,
        string unitCode,
        bool isWithinTolerance)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionSampleId = DomainRules.Id(inspectionSampleId, nameof(inspectionSampleId));
        CriterionId = DomainRules.Id(criterionId, nameof(criterionId));
        MeasuredValue = DomainRules.Nonnegative(measuredValue, nameof(measuredValue));
        UnitCode = DomainRules.Text(unitCode, nameof(unitCode));
        IsWithinTolerance = isWithinTolerance;
    }

    public Guid InspectionSampleId { get; }
    public Guid CriterionId { get; }
    public decimal MeasuredValue { get; }
    public string UnitCode { get; }
    public bool IsWithinTolerance { get; }
}
