namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionMaterialReturn record.</summary>
public sealed class ProductionMaterialReturn : OrganizationEntity
{
    public ProductionMaterialReturn(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid inventoryItemId,
        decimal returnedQuantity,
        DateOnly returnedOn,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        ReturnedQuantity = DomainRules.Positive(returnedQuantity, nameof(returnedQuantity));
        ReturnedOn = DomainRules.Date(returnedOn, nameof(returnedOn));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid WorkOrderId { get; }
    public Guid InventoryItemId { get; }
    public decimal ReturnedQuantity { get; }
    public DateOnly ReturnedOn { get; }
    public string Reason { get; }
}
