namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentAuthorization; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentAuthorization : OrganizationEntity
{
    public PaymentAuthorization(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string externalPaymentId,
        Guid counterpartyId,
        decimal authorizedAmount,
        string currency,
        DateTimeOffset authorizedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ExternalPaymentId = DomainRules.Text(externalPaymentId, nameof(externalPaymentId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        AuthorizedAmount = DomainRules.Positive(authorizedAmount, nameof(authorizedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        AuthorizedAtUtc = DomainRules.Instant(authorizedAtUtc, nameof(authorizedAtUtc));
    }

    public string ExternalPaymentId { get; }
    public Guid CounterpartyId { get; }
    public decimal AuthorizedAmount { get; }
    public string Currency { get; }
    public DateTimeOffset AuthorizedAtUtc { get; }
}
