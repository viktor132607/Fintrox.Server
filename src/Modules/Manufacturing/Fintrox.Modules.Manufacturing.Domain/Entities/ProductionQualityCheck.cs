namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionQualityCheck record.</summary>
public sealed class ProductionQualityCheck : OrganizationEntity
{
    public ProductionQualityCheck(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid inspectionPlanId,
        DateOnly inspectedOn,
        string result)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        InspectionPlanId = DomainRules.Id(inspectionPlanId, nameof(inspectionPlanId));
        InspectedOn = DomainRules.Date(inspectedOn, nameof(inspectedOn));
        Result = DomainRules.Text(result, nameof(result));
    }

    public Guid WorkOrderId { get; }
    public Guid InspectionPlanId { get; }
    public DateOnly InspectedOn { get; }
    public string Result { get; }
}
