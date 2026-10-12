namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyLegalIdentifier. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyLegalIdentifier : OrganizationEntity
{
    public CounterpartyLegalIdentifier(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string identifierType,
        string identifierValue,
        string countryCode,
        bool isVerified)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        IdentifierType = DomainRules.Text(identifierType, nameof(identifierType));
        IdentifierValue = DomainRules.Text(identifierValue, nameof(identifierValue));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        IsVerified = isVerified;
    }

    public Guid CounterpartyId { get; }
    public string IdentifierType { get; }
    public string IdentifierValue { get; }
    public string CountryCode { get; }
    public bool IsVerified { get; }
}
