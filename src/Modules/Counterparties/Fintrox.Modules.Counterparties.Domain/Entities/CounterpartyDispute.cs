namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyDispute. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyDispute : OrganizationEntity
{
    public CounterpartyDispute(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string reference,
        DateOnly openedOn,
        decimal disputedAmount,
        string currency,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Reference = DomainRules.Text(reference, nameof(reference));
        OpenedOn = DomainRules.Date(openedOn, nameof(openedOn));
        DisputedAmount = DomainRules.Nonnegative(disputedAmount, nameof(disputedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid CounterpartyId { get; }
    public string Reference { get; }
    public DateOnly OpenedOn { get; }
    public decimal DisputedAmount { get; }
    public string Currency { get; }
    public string Status { get; }
}
