namespace Fintrox.Modules.Payments.Domain.Entities;

public sealed class PaymentAdvance : OrganizationEntity
{
    public PaymentAdvance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid paymentId,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid CounterpartyId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
