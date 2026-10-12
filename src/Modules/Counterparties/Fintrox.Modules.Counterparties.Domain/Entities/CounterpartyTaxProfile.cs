namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyTaxProfile. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyTaxProfile : OrganizationEntity
{
    public CounterpartyTaxProfile(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string taxRegistrationNumber,
        string countryCode,
        string taxCategory,
        DateOnly validFrom)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        TaxRegistrationNumber = DomainRules.Text(taxRegistrationNumber, nameof(taxRegistrationNumber));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        TaxCategory = DomainRules.Text(taxCategory, nameof(taxCategory));
        ValidFrom = DomainRules.Date(validFrom, nameof(validFrom));
    }

    public Guid CounterpartyId { get; }
    public string TaxRegistrationNumber { get; }
    public string CountryCode { get; }
    public string TaxCategory { get; }
    public DateOnly ValidFrom { get; }
}
