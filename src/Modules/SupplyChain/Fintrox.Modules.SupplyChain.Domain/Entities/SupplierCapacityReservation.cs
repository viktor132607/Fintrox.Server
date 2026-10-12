namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplierCapacityReservation domain record. No cross-module navigation properties.</summary>
public sealed class SupplierCapacityReservation : OrganizationEntity
{
    public SupplierCapacityReservation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierCapacityPlanId,
        Guid purchaseOrderId,
        decimal reservedQuantity,
        DateOnly reservedOn)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierCapacityPlanId = DomainRules.Id(supplierCapacityPlanId, nameof(supplierCapacityPlanId));
        PurchaseOrderId = DomainRules.Id(purchaseOrderId, nameof(purchaseOrderId));
        ReservedQuantity = DomainRules.Positive(reservedQuantity, nameof(reservedQuantity));
        ReservedOn = DomainRules.Date(reservedOn, nameof(reservedOn));
    }

    public Guid SupplierCapacityPlanId { get; }
    public Guid PurchaseOrderId { get; }
    public decimal ReservedQuantity { get; }
    public DateOnly ReservedOn { get; }
}
