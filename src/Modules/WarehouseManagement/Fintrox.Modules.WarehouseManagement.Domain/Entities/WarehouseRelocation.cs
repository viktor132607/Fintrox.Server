namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseRelocation; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseRelocation : OrganizationEntity
{
    public WarehouseRelocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid fromBinId,
        Guid toBinId,
        decimal quantity,
        DateOnly transferredOn)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        FromBinId = DomainRules.Id(fromBinId, nameof(fromBinId));
        ToBinId = DomainRules.Id(toBinId, nameof(toBinId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        TransferredOn = DomainRules.Date(transferredOn, nameof(transferredOn));
    }

    public Guid InventoryItemId { get; }
    public Guid FromBinId { get; }
    public Guid ToBinId { get; }
    public decimal Quantity { get; }
    public DateOnly TransferredOn { get; }
}
