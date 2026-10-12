namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueContractCostAmortization model; no cross-module navigations.</summary>
public sealed class RevenueContractCostAmortization : OrganizationEntity
{
    public RevenueContractCostAmortization(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractCostId,
        DateOnly amortizationOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractCostId = DomainRules.Id(revenueContractCostId, nameof(revenueContractCostId));
        AmortizationOn = DomainRules.Date(amortizationOn, nameof(amortizationOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid RevenueContractCostId { get; }
    public DateOnly AmortizationOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
