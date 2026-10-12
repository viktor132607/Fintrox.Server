namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualitySupplierClaim model. References require application validation.</summary>
public sealed class QualitySupplierClaim : OrganizationEntity
{
    public QualitySupplierClaim(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        Guid goodsReceiptId,
        string claimReference,
        decimal claimAmount,
        string currency,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        GoodsReceiptId = DomainRules.Id(goodsReceiptId, nameof(goodsReceiptId));
        ClaimReference = DomainRules.Text(claimReference, nameof(claimReference));
        ClaimAmount = DomainRules.Nonnegative(claimAmount, nameof(claimAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SupplierId { get; }
    public Guid GoodsReceiptId { get; }
    public string ClaimReference { get; }
    public decimal ClaimAmount { get; }
    public string Currency { get; }
    public string Status { get; }
}
