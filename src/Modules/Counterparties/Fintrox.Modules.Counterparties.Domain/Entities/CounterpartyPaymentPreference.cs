namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyPaymentPreference. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyPaymentPreference : OrganizationEntity
{
    public CounterpartyPaymentPreference(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid paymentTermId,
        string currency,
        string preferredMethod,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        PaymentTermId = DomainRules.Id(paymentTermId, nameof(paymentTermId));
        Currency = DomainRules.Currency(currency, nameof(currency));
        PreferredMethod = DomainRules.Text(preferredMethod, nameof(preferredMethod));
        IsActive = isActive;
    }

    public Guid CounterpartyId { get; }
    public Guid PaymentTermId { get; }
    public string Currency { get; }
    public string PreferredMethod { get; }
    public bool IsActive { get; }
}
