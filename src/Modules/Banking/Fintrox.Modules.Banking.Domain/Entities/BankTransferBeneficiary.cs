namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankTransferBeneficiary record; cross-module references require application authorization.</summary>
public sealed class BankTransferBeneficiary : OrganizationEntity
{
    public BankTransferBeneficiary(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string beneficiaryName,
        string iban,
        string bic,
        string countryCode)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        BeneficiaryName = DomainRules.Text(beneficiaryName, nameof(beneficiaryName));
        Iban = DomainRules.Text(iban, nameof(iban));
        Bic = DomainRules.Text(bic, nameof(bic));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
    }

    public Guid CounterpartyId { get; }
    public string BeneficiaryName { get; }
    public string Iban { get; }
    public string Bic { get; }
    public string CountryCode { get; }
}
