namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesCollectionCase model; no cross-module navigations.</summary>
public sealed class SalesCollectionCase : OrganizationEntity
{
    public SalesCollectionCase(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid invoiceId,
        DateOnly openedOn,
        decimal outstandingAmount,
        string currency,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        InvoiceId = DomainRules.Id(invoiceId, nameof(invoiceId));
        OpenedOn = DomainRules.Date(openedOn, nameof(openedOn));
        OutstandingAmount = DomainRules.Nonnegative(outstandingAmount, nameof(outstandingAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid CounterpartyId { get; }
    public Guid InvoiceId { get; }
    public DateOnly OpenedOn { get; }
    public decimal OutstandingAmount { get; }
    public string Currency { get; }
    public string Status { get; }
}
