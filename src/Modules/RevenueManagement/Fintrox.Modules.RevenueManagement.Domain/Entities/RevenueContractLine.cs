namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueContractLine model; no cross-module navigations.</summary>
public sealed class RevenueContractLine : OrganizationEntity
{
    public RevenueContractLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        Guid productId,
        decimal quantity,
        decimal transactionPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        ProductId = DomainRules.Id(productId, nameof(productId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        TransactionPrice = DomainRules.Nonnegative(transactionPrice, nameof(transactionPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid RevenueContractId { get; }
    public Guid ProductId { get; }
    public decimal Quantity { get; }
    public decimal TransactionPrice { get; }
    public string Currency { get; }
}
