namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueContractCost model; no cross-module navigations.</summary>
public sealed class RevenueContractCost : OrganizationEntity
{
    public RevenueContractCost(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        string costType,
        decimal amount,
        string currency,
        DateOnly incurredOn)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        CostType = DomainRules.Text(costType, nameof(costType));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        IncurredOn = DomainRules.Date(incurredOn, nameof(incurredOn));
    }

    public Guid RevenueContractId { get; }
    public string CostType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public DateOnly IncurredOn { get; }
}
