namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentSettlementLine; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentSettlementLine : OrganizationEntity
{
    public PaymentSettlementLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid settlementId,
        Guid providerTransactionId,
        decimal settlementAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SettlementId = DomainRules.Id(settlementId, nameof(settlementId));
        ProviderTransactionId = DomainRules.Id(providerTransactionId, nameof(providerTransactionId));
        SettlementAmount = DomainRules.Nonzero(settlementAmount, nameof(settlementAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SettlementId { get; }
    public Guid ProviderTransactionId { get; }
    public decimal SettlementAmount { get; }
    public string Currency { get; }
}
