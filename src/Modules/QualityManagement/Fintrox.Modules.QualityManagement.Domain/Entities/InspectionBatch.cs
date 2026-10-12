namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped InspectionBatch model. References require application validation.</summary>
public sealed class InspectionBatch : OrganizationEntity
{
    public InspectionBatch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionPlanId,
        Guid inventoryLotId,
        string batchReference,
        DateOnly inspectedOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionPlanId = DomainRules.Id(inspectionPlanId, nameof(inspectionPlanId));
        InventoryLotId = DomainRules.Id(inventoryLotId, nameof(inventoryLotId));
        BatchReference = DomainRules.Text(batchReference, nameof(batchReference));
        InspectedOn = DomainRules.Date(inspectedOn, nameof(inspectedOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid InspectionPlanId { get; }
    public Guid InventoryLotId { get; }
    public string BatchReference { get; }
    public DateOnly InspectedOn { get; }
    public string Status { get; }
}
