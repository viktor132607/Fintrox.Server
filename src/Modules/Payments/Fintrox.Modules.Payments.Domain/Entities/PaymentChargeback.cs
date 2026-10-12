namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentChargeback; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentChargeback : OrganizationEntity
{
    public PaymentChargeback(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentId,
        string externalCaseId,
        decimal disputedAmount,
        string currency,
        DateOnly openedOn)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        ExternalCaseId = DomainRules.Text(externalCaseId, nameof(externalCaseId));
        DisputedAmount = DomainRules.Positive(disputedAmount, nameof(disputedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        OpenedOn = DomainRules.Date(openedOn, nameof(openedOn));
    }

    public Guid PaymentId { get; }
    public string ExternalCaseId { get; }
    public decimal DisputedAmount { get; }
    public string Currency { get; }
    public DateOnly OpenedOn { get; }
}
