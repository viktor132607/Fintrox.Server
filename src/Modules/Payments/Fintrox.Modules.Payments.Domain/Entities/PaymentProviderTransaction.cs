namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentProviderTransaction; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentProviderTransaction : OrganizationEntity
{
    public PaymentProviderTransaction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid providerConnectionId,
        string externalTransactionId,
        string transactionType,
        decimal amount,
        string currency,
        DateTimeOffset occurredAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ProviderConnectionId = DomainRules.Id(providerConnectionId, nameof(providerConnectionId));
        ExternalTransactionId = DomainRules.Text(externalTransactionId, nameof(externalTransactionId));
        TransactionType = DomainRules.Text(transactionType, nameof(transactionType));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
    }

    public Guid ProviderConnectionId { get; }
    public string ExternalTransactionId { get; }
    public string TransactionType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}
