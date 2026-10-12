namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned MinorityInterest; external references validated by application layer.</summary>
public sealed class MinorityInterest : OrganizationEntity
{
    public MinorityInterest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        Guid subsidiaryCompanyId,
        decimal ownershipPercent,
        decimal nonControllingAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        SubsidiaryCompanyId = DomainRules.Id(subsidiaryCompanyId, nameof(subsidiaryCompanyId));
        OwnershipPercent = DomainRules.Percent(ownershipPercent, nameof(ownershipPercent));
        NonControllingAmount = DomainRules.Nonnegative(nonControllingAmount, nameof(nonControllingAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid CompanyGroupId { get; }
    public Guid SubsidiaryCompanyId { get; }
    public decimal OwnershipPercent { get; }
    public decimal NonControllingAmount { get; }
    public string Currency { get; }
}
