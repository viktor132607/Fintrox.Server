namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionRevenueAllocation domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionRevenueAllocation : OrganizationEntity
{
    public SubscriptionRevenueAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        Guid revenueContractId,
        decimal recognizedAmount,
        string currency,
        DateOnly asOfDate)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        RecognizedAmount = DomainRules.Nonnegative(recognizedAmount, nameof(recognizedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
    }

    public Guid SubscriptionId { get; }
    public Guid RevenueContractId { get; }
    public decimal RecognizedAmount { get; }
    public string Currency { get; }
    public DateOnly AsOfDate { get; }
}
