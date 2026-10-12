namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankBranch record; cross-module references require application authorization.</summary>
public sealed class BankBranch : OrganizationEntity
{
    public BankBranch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string bankName,
        string bic,
        string countryCode,
        string branchCode)
        : base(id, organizationId, createdAtUtc)
    {
        BankName = DomainRules.Text(bankName, nameof(bankName));
        Bic = DomainRules.Text(bic, nameof(bic));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        BranchCode = DomainRules.Text(branchCode, nameof(branchCode));
    }

    public string BankName { get; }
    public string Bic { get; }
    public string CountryCode { get; }
    public string BranchCode { get; }
}
