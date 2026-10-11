namespace Fintrox.Modules.Counterparties.Domain.Entities;

public sealed class CounterpartyBankAccount : OrganizationEntity
{
    public CounterpartyBankAccount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string accountIdentifier,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        AccountIdentifier = DomainRules.Text(accountIdentifier, nameof(accountIdentifier));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid CounterpartyId { get; }
    public string AccountIdentifier { get; }
    public string Currency { get; }
}
