namespace Fintrox.Modules.Banking.Domain.Entities;

public sealed class BankAccount : OrganizationEntity
{
    public BankAccount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        string accountIdentifier,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        AccountIdentifier = DomainRules.Text(accountIdentifier, nameof(accountIdentifier));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public string Name { get; }
    public string AccountIdentifier { get; }
    public string Currency { get; }
}
