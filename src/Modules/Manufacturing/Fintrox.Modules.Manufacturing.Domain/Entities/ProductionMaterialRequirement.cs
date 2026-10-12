namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionMaterialRequirement record.</summary>
public sealed class ProductionMaterialRequirement : OrganizationEntity
{
    public ProductionMaterialRequirement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid inventoryItemId,
        decimal requiredQuantity,
        DateOnly requiredOn)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        RequiredQuantity = DomainRules.Positive(requiredQuantity, nameof(requiredQuantity));
        RequiredOn = DomainRules.Date(requiredOn, nameof(requiredOn));
    }

    public Guid WorkOrderId { get; }
    public Guid InventoryItemId { get; }
    public decimal RequiredQuantity { get; }
    public DateOnly RequiredOn { get; }
}
