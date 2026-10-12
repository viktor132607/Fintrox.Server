namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable AccruedRevenueBalance model; no cross-module navigations.</summary>
public sealed class AccruedRevenueBalance : OrganizationEntity
{
    public AccruedRevenueBalance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        DateOnly asOfDate,
        decimal accruedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
        AccruedAmount = DomainRules.Nonnegative(accruedAmount, nameof(accruedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid RevenueContractId { get; }
    public DateOnly AsOfDate { get; }
    public decimal AccruedAmount { get; }
    public string Currency { get; }
}
