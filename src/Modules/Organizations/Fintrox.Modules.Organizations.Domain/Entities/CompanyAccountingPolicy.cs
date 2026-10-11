namespace Fintrox.Modules.Organizations.Domain.Entities;

public sealed class CompanyAccountingPolicy : OrganizationEntity
{
    public CompanyAccountingPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        DateOnly effectiveFrom,
        string accountingStandard,
        string functionalCurrency)
        : base(id, organizationId, createdAtUtc)
    {
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        AccountingStandard = DomainRules.Text(accountingStandard, nameof(accountingStandard));
        FunctionalCurrency = DomainRules.Currency(functionalCurrency, nameof(functionalCurrency));
    }

    public DateOnly EffectiveFrom { get; }
    public string AccountingStandard { get; }
    public string FunctionalCurrency { get; }
}
