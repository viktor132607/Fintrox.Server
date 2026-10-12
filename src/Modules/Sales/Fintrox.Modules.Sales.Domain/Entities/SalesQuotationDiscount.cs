namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesQuotationDiscount model; no cross-module navigations.</summary>
public sealed class SalesQuotationDiscount : OrganizationEntity
{
    public SalesQuotationDiscount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesQuotationLineId,
        string discountType,
        decimal discountPercent,
        decimal discountAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SalesQuotationLineId = DomainRules.Id(salesQuotationLineId, nameof(salesQuotationLineId));
        DiscountType = DomainRules.Text(discountType, nameof(discountType));
        DiscountPercent = DomainRules.Percent(discountPercent, nameof(discountPercent));
        DiscountAmount = DomainRules.Nonnegative(discountAmount, nameof(discountAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SalesQuotationLineId { get; }
    public string DiscountType { get; }
    public decimal DiscountPercent { get; }
    public decimal DiscountAmount { get; }
    public string Currency { get; }
}
