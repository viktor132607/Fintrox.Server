namespace Fintrox.Modules.Commerce.Domain.Entities;

public sealed class CommerceOrder : OrganizationEntity
{
    public CommerceOrder(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storefrontId,
        string externalOrderId,
        Guid counterpartyId,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        StorefrontId = DomainRules.Id(storefrontId, nameof(storefrontId));
        ExternalOrderId = DomainRules.Text(externalOrderId, nameof(externalOrderId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid StorefrontId { get; }
    public string ExternalOrderId { get; }
    public Guid CounterpartyId { get; }
    public string Currency { get; }
}
