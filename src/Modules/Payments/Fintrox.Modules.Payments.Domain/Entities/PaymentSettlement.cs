namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentSettlement; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentSettlement : OrganizationEntity
{
    public PaymentSettlement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid providerConnectionId,
        string settlementReference,
        DateOnly settledOn,
        decimal grossAmount,
        decimal feeAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ProviderConnectionId = DomainRules.Id(providerConnectionId, nameof(providerConnectionId));
        SettlementReference = DomainRules.Text(settlementReference, nameof(settlementReference));
        SettledOn = DomainRules.Date(settledOn, nameof(settledOn));
        GrossAmount = DomainRules.Nonnegative(grossAmount, nameof(grossAmount));
        FeeAmount = DomainRules.Nonnegative(feeAmount, nameof(feeAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ProviderConnectionId { get; }
    public string SettlementReference { get; }
    public DateOnly SettledOn { get; }
    public decimal GrossAmount { get; }
    public decimal FeeAmount { get; }
    public string Currency { get; }
}
