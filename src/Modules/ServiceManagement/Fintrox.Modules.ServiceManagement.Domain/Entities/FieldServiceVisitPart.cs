namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped FieldServiceVisitPart domain record. No cross-module navigation properties.</summary>
public sealed class FieldServiceVisitPart : OrganizationEntity
{
    public FieldServiceVisitPart(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fieldServiceVisitId,
        Guid inventoryItemId,
        decimal quantityUsed,
        decimal unitCost,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        FieldServiceVisitId = DomainRules.Id(fieldServiceVisitId, nameof(fieldServiceVisitId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        QuantityUsed = DomainRules.Positive(quantityUsed, nameof(quantityUsed));
        UnitCost = DomainRules.Nonnegative(unitCost, nameof(unitCost));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid FieldServiceVisitId { get; }
    public Guid InventoryItemId { get; }
    public decimal QuantityUsed { get; }
    public decimal UnitCost { get; }
    public string Currency { get; }
}
