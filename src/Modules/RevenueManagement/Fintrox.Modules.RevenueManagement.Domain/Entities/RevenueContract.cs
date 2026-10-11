namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

public sealed class RevenueContract : OrganizationEntity
{
    public RevenueContract(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        Guid counterpartyId,
        decimal totalAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        TotalAmount = DomainRules.Nonnegative(totalAmount, nameof(totalAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid BusinessContractId { get; }
    public Guid CounterpartyId { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }
}
