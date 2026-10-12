namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable PerformanceObligationAllocation model; no cross-module navigations.</summary>
public sealed class PerformanceObligationAllocation : OrganizationEntity
{
    public PerformanceObligationAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid performanceObligationId,
        Guid contractLineId,
        decimal allocatedPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PerformanceObligationId = DomainRules.Id(performanceObligationId, nameof(performanceObligationId));
        ContractLineId = DomainRules.Id(contractLineId, nameof(contractLineId));
        AllocatedPrice = DomainRules.Nonnegative(allocatedPrice, nameof(allocatedPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PerformanceObligationId { get; }
    public Guid ContractLineId { get; }
    public decimal AllocatedPrice { get; }
    public string Currency { get; }
}
