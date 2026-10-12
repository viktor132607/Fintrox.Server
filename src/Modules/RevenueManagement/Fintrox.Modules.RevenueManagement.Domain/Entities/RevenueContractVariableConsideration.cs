namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueContractVariableConsideration model; no cross-module navigations.</summary>
public sealed class RevenueContractVariableConsideration : OrganizationEntity
{
    public RevenueContractVariableConsideration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        string considerationType,
        decimal estimatedAmount,
        string currency,
        DateOnly estimatedOn)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        ConsiderationType = DomainRules.Text(considerationType, nameof(considerationType));
        EstimatedAmount = DomainRules.Nonnegative(estimatedAmount, nameof(estimatedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        EstimatedOn = DomainRules.Date(estimatedOn, nameof(estimatedOn));
    }

    public Guid RevenueContractId { get; }
    public string ConsiderationType { get; }
    public decimal EstimatedAmount { get; }
    public string Currency { get; }
    public DateOnly EstimatedOn { get; }
}
