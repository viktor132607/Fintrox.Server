namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentCapture; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentCapture : OrganizationEntity
{
    public PaymentCapture(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentAuthorizationId,
        decimal capturedAmount,
        string currency,
        DateTimeOffset capturedAtUtc,
        string externalReference)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentAuthorizationId = DomainRules.Id(paymentAuthorizationId, nameof(paymentAuthorizationId));
        CapturedAmount = DomainRules.Positive(capturedAmount, nameof(capturedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        CapturedAtUtc = DomainRules.Instant(capturedAtUtc, nameof(capturedAtUtc));
        ExternalReference = DomainRules.Text(externalReference, nameof(externalReference));
    }

    public Guid PaymentAuthorizationId { get; }
    public decimal CapturedAmount { get; }
    public string Currency { get; }
    public DateTimeOffset CapturedAtUtc { get; }
    public string ExternalReference { get; }
}
