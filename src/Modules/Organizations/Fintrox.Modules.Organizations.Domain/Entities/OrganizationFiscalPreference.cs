namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationFiscalPreference record.</summary>
public sealed class OrganizationFiscalPreference : OrganizationEntity
{
    public OrganizationFiscalPreference(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        int fiscalYearStartMonth,
        string baseCurrency,
        string accountingPolicyCode)
        : base(id, organizationId, createdAtUtc)
    {
        FiscalYearStartMonth = DomainRules.PositiveInt(fiscalYearStartMonth, nameof(fiscalYearStartMonth));
        BaseCurrency = DomainRules.Currency(baseCurrency, nameof(baseCurrency));
        AccountingPolicyCode = DomainRules.Text(accountingPolicyCode, nameof(accountingPolicyCode));
    }

    public int FiscalYearStartMonth { get; }
    public string BaseCurrency { get; }
    public string AccountingPolicyCode { get; }
}
