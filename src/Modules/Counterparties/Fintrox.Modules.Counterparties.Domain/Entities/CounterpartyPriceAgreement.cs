namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyPriceAgreement. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyPriceAgreement : OrganizationEntity
{
    public CounterpartyPriceAgreement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid priceListId,
        DateOnly startsOn,
        DateOnly endsOn,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        PriceListId = DomainRules.Id(priceListId, nameof(priceListId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid CounterpartyId { get; }
    public Guid PriceListId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string Currency { get; }
}
