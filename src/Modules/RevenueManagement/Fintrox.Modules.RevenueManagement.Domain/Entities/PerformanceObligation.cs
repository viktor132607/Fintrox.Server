namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

public sealed class PerformanceObligation : OrganizationEntity
{
    public PerformanceObligation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        string description,
        decimal allocatedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        Description = DomainRules.Text(description, nameof(description));
        AllocatedAmount = DomainRules.Nonnegative(allocatedAmount, nameof(allocatedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid RevenueContractId { get; }
    public string Description { get; }
    public decimal AllocatedAmount { get; }
    public string Currency { get; }
}
