namespace Fintrox.Modules.Tax.Domain.Entities;

public sealed class TaxJurisdiction : OrganizationEntity
{
    public TaxJurisdiction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string countryCode)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
    }

    public string Code { get; }
    public string Name { get; }
    public string CountryCode { get; }
}
