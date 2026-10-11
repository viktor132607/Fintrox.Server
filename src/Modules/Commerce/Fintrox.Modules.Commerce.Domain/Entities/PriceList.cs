namespace Fintrox.Modules.Commerce.Domain.Entities;

public sealed class PriceList : OrganizationEntity
{
    public PriceList(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public string Name { get; }
    public string Currency { get; }
}
