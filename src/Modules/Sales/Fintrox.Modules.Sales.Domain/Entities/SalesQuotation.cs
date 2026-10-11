namespace Fintrox.Modules.Sales.Domain.Entities;

public sealed class SalesQuotation : OrganizationEntity
{
    public SalesQuotation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string reference,
        DateOnly validUntil,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Reference = DomainRules.Text(reference, nameof(reference));
        ValidUntil = DomainRules.Date(validUntil, nameof(validUntil));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid CounterpartyId { get; }
    public string Reference { get; }
    public DateOnly ValidUntil { get; }
    public string Currency { get; }
}
