namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplierLeadTime domain record. No cross-module navigation properties.</summary>
public sealed class SupplierLeadTime : OrganizationEntity
{
    public SupplierLeadTime(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        Guid inventoryItemId,
        int leadTimeDays,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        LeadTimeDays = DomainRules.PositiveInt(leadTimeDays, nameof(leadTimeDays));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid SupplierId { get; }
    public Guid InventoryItemId { get; }
    public int LeadTimeDays { get; }
    public DateOnly EffectiveFrom { get; }
}
