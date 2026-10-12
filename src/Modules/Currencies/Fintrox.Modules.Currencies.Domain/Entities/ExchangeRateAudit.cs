namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped ExchangeRateAudit. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class ExchangeRateAudit : OrganizationEntity
{
    public ExchangeRateAudit(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid exchangeRateProviderId,
        Guid currencyPairId,
        decimal rate,
        DateTimeOffset changedAtUtc,
        Guid changedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        ExchangeRateProviderId = DomainRules.Id(exchangeRateProviderId, nameof(exchangeRateProviderId));
        CurrencyPairId = DomainRules.Id(currencyPairId, nameof(currencyPairId));
        Rate = DomainRules.Positive(rate, nameof(rate));
        ChangedAtUtc = DomainRules.Instant(changedAtUtc, nameof(changedAtUtc));
        ChangedByUserId = DomainRules.Id(changedByUserId, nameof(changedByUserId));
    }

    public Guid ExchangeRateProviderId { get; }
    public Guid CurrencyPairId { get; }
    public decimal Rate { get; }
    public DateTimeOffset ChangedAtUtc { get; }
    public Guid ChangedByUserId { get; }
}
