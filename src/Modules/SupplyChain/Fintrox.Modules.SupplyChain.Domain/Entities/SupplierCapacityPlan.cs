namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplierCapacityPlan domain record. No cross-module navigation properties.</summary>
public sealed class SupplierCapacityPlan : OrganizationEntity
{
    public SupplierCapacityPlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        Guid inventoryItemId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal maximumQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        MaximumQuantity = DomainRules.Nonnegative(maximumQuantity, nameof(maximumQuantity));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid SupplierId { get; }
    public Guid InventoryItemId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal MaximumQuantity { get; }
}
