namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped SupplierReturn model. References require application validation.</summary>
public sealed class SupplierReturn : OrganizationEntity
{
    public SupplierReturn(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        Guid goodsReceiptId,
        string returnNumber,
        DateOnly requestedOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        GoodsReceiptId = DomainRules.Id(goodsReceiptId, nameof(goodsReceiptId));
        ReturnNumber = DomainRules.Text(returnNumber, nameof(returnNumber));
        RequestedOn = DomainRules.Date(requestedOn, nameof(requestedOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SupplierId { get; }
    public Guid GoodsReceiptId { get; }
    public string ReturnNumber { get; }
    public DateOnly RequestedOn { get; }
    public string Status { get; }
}
