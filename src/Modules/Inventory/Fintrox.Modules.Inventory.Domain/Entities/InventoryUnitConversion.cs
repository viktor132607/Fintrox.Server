namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryUnitConversion domain record; references checked in application layer.</summary>
public sealed class InventoryUnitConversion : OrganizationEntity
{
    public InventoryUnitConversion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        string fromUnitCode,
        string toUnitCode,
        decimal factor)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        FromUnitCode = DomainRules.Text(fromUnitCode, nameof(fromUnitCode));
        ToUnitCode = DomainRules.Text(toUnitCode, nameof(toUnitCode));
        Factor = DomainRules.Positive(factor, nameof(factor));
    }

    public Guid InventoryItemId { get; }
    public string FromUnitCode { get; }
    public string ToUnitCode { get; }
    public decimal Factor { get; }
}
