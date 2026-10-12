namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplierShipmentNotice domain record. No cross-module navigation properties.</summary>
public sealed class SupplierShipmentNotice : OrganizationEntity
{
    public SupplierShipmentNotice(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        Guid purchaseOrderId,
        string noticeReference,
        DateOnly estimatedArrivalOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        PurchaseOrderId = DomainRules.Id(purchaseOrderId, nameof(purchaseOrderId));
        NoticeReference = DomainRules.Text(noticeReference, nameof(noticeReference));
        EstimatedArrivalOn = DomainRules.Date(estimatedArrivalOn, nameof(estimatedArrivalOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SupplierId { get; }
    public Guid PurchaseOrderId { get; }
    public string NoticeReference { get; }
    public DateOnly EstimatedArrivalOn { get; }
    public string Status { get; }
}
