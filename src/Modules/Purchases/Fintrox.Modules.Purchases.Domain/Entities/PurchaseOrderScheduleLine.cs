namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseOrderScheduleLine model. References require application validation.</summary>
public sealed class PurchaseOrderScheduleLine : OrganizationEntity
{
    public PurchaseOrderScheduleLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseOrderLineId,
        DateOnly expectedOn,
        decimal scheduledQuantity,
        Guid deliveryLocationId)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseOrderLineId = DomainRules.Id(purchaseOrderLineId, nameof(purchaseOrderLineId));
        ExpectedOn = DomainRules.Date(expectedOn, nameof(expectedOn));
        ScheduledQuantity = DomainRules.Positive(scheduledQuantity, nameof(scheduledQuantity));
        DeliveryLocationId = DomainRules.Id(deliveryLocationId, nameof(deliveryLocationId));
    }

    public Guid PurchaseOrderLineId { get; }
    public DateOnly ExpectedOn { get; }
    public decimal ScheduledQuantity { get; }
    public Guid DeliveryLocationId { get; }
}
