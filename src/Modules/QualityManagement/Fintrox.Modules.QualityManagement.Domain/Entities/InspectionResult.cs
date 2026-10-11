namespace Fintrox.Modules.QualityManagement.Domain.Entities;

public sealed class InspectionResult : OrganizationEntity
{
    public InspectionResult(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionId,
        Guid criterionId,
        decimal measuredValue)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionId = DomainRules.Id(inspectionId, nameof(inspectionId));
        CriterionId = DomainRules.Id(criterionId, nameof(criterionId));
        MeasuredValue = measuredValue;
    }

    public Guid InspectionId { get; }
    public Guid CriterionId { get; }
    public decimal MeasuredValue { get; }
}
