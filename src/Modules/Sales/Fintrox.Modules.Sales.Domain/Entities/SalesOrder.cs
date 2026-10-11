namespace Fintrox.Modules.Sales.Domain.Entities;

public sealed class SalesOrder : OrganizationEntity
{
    public SalesOrder(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string reference,
        DateOnly orderedOn,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Reference = DomainRules.Text(reference, nameof(reference));
        OrderedOn = DomainRules.Date(orderedOn, nameof(orderedOn));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid CounterpartyId { get; }
    public string Reference { get; }
    public DateOnly OrderedOn { get; }
    public string Currency { get; }
}
