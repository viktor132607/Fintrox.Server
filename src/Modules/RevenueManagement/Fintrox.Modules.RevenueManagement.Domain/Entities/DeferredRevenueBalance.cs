namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable DeferredRevenueBalance model; no cross-module navigations.</summary>
public sealed class DeferredRevenueBalance : OrganizationEntity
{
    public DeferredRevenueBalance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        DateOnly asOfDate,
        decimal deferredAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
        DeferredAmount = DomainRules.Nonnegative(deferredAmount, nameof(deferredAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid RevenueContractId { get; }
    public DateOnly AsOfDate { get; }
    public decimal DeferredAmount { get; }
    public string Currency { get; }
}
