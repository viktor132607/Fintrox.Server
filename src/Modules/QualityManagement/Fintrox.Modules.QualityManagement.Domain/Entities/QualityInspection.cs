namespace Fintrox.Modules.QualityManagement.Domain.Entities;

public sealed class QualityInspection : OrganizationEntity
{
    public QualityInspection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionPlanId,
        Guid sourceDocumentId,
        DateOnly inspectedOn)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionPlanId = DomainRules.Id(inspectionPlanId, nameof(inspectionPlanId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        InspectedOn = DomainRules.Date(inspectedOn, nameof(inspectedOn));
    }

    public Guid InspectionPlanId { get; }
    public Guid SourceDocumentId { get; }
    public DateOnly InspectedOn { get; }
}
