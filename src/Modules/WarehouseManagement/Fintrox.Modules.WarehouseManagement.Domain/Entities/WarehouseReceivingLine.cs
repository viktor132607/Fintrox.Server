namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseReceivingLine; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseReceivingLine : OrganizationEntity
{
    public WarehouseReceivingLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid receivingTaskId,
        Guid inventoryItemId,
        decimal expectedQuantity,
        decimal receivedQuantity,
        string unitCode)
        : base(id, organizationId, createdAtUtc)
    {
        ReceivingTaskId = DomainRules.Id(receivingTaskId, nameof(receivingTaskId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        ExpectedQuantity = DomainRules.Nonnegative(expectedQuantity, nameof(expectedQuantity));
        ReceivedQuantity = DomainRules.Nonnegative(receivedQuantity, nameof(receivedQuantity));
        UnitCode = DomainRules.Text(unitCode, nameof(unitCode));
    }

    public Guid ReceivingTaskId { get; }
    public Guid InventoryItemId { get; }
    public decimal ExpectedQuantity { get; }
    public decimal ReceivedQuantity { get; }
    public string UnitCode { get; }
}
