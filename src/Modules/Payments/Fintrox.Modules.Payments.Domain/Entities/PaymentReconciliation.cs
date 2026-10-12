namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentReconciliation; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentReconciliation : OrganizationEntity
{
    public PaymentReconciliation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentId,
        Guid bankStatementLineId,
        decimal matchedAmount,
        string currency,
        DateTimeOffset matchedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        BankStatementLineId = DomainRules.Id(bankStatementLineId, nameof(bankStatementLineId));
        MatchedAmount = DomainRules.Positive(matchedAmount, nameof(matchedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        MatchedAtUtc = DomainRules.Instant(matchedAtUtc, nameof(matchedAtUtc));
    }

    public Guid PaymentId { get; }
    public Guid BankStatementLineId { get; }
    public decimal MatchedAmount { get; }
    public string Currency { get; }
    public DateTimeOffset MatchedAtUtc { get; }
}
