namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueContractDiscount model; no cross-module navigations.</summary>
public sealed class RevenueContractDiscount : OrganizationEntity
{
    public RevenueContractDiscount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        string discountType,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        DiscountType = DomainRules.Text(discountType, nameof(discountType));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid RevenueContractId { get; }
    public string DiscountType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
