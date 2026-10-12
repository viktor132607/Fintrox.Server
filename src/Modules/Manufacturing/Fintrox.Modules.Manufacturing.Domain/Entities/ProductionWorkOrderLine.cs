namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionWorkOrderLine record.</summary>
public sealed class ProductionWorkOrderLine : OrganizationEntity
{
    public ProductionWorkOrderLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid inventoryItemId,
        decimal plannedQuantity,
        decimal producedQuantity,
        decimal scrappedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        PlannedQuantity = DomainRules.Positive(plannedQuantity, nameof(plannedQuantity));
        ProducedQuantity = DomainRules.Nonnegative(producedQuantity, nameof(producedQuantity));
        ScrappedQuantity = DomainRules.Nonnegative(scrappedQuantity, nameof(scrappedQuantity));
    }

    public Guid WorkOrderId { get; }
    public Guid InventoryItemId { get; }
    public decimal PlannedQuantity { get; }
    public decimal ProducedQuantity { get; }
    public decimal ScrappedQuantity { get; }
}
