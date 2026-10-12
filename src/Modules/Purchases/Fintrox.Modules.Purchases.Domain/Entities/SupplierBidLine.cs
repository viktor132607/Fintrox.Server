namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped SupplierBidLine model. References require application validation.</summary>
public sealed class SupplierBidLine : OrganizationEntity
{
    public SupplierBidLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierBidId,
        Guid requestLineId,
        decimal unitPrice,
        decimal offeredQuantity,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierBidId = DomainRules.Id(supplierBidId, nameof(supplierBidId));
        RequestLineId = DomainRules.Id(requestLineId, nameof(requestLineId));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        OfferedQuantity = DomainRules.Positive(offeredQuantity, nameof(offeredQuantity));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SupplierBidId { get; }
    public Guid RequestLineId { get; }
    public decimal UnitPrice { get; }
    public decimal OfferedQuantity { get; }
    public string Currency { get; }
}
