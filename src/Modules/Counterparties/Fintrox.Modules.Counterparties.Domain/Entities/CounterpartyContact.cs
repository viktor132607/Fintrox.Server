namespace Fintrox.Modules.Counterparties.Domain.Entities;

public sealed class CounterpartyContact : OrganizationEntity
{
    public CounterpartyContact(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string name,
        string email)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Name = DomainRules.Text(name, nameof(name));
        Email = DomainRules.Text(email, nameof(email));
    }

    public Guid CounterpartyId { get; }
    public string Name { get; }
    public string Email { get; }
}
