namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentDispute; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentDispute : OrganizationEntity
{
    public PaymentDispute(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentId,
        Guid counterpartyId,
        decimal disputedAmount,
        string currency,
        DateTimeOffset openedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        DisputedAmount = DomainRules.Positive(disputedAmount, nameof(disputedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        OpenedAtUtc = DomainRules.Instant(openedAtUtc, nameof(openedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid PaymentId { get; }
    public Guid CounterpartyId { get; }
    public decimal DisputedAmount { get; }
    public string Currency { get; }
    public DateTimeOffset OpenedAtUtc { get; }
    public string Status { get; }
}
