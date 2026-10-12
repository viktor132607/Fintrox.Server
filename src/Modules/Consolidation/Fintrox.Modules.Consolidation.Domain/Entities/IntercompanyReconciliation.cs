namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned IntercompanyReconciliation; external references validated by application layer.</summary>
public sealed class IntercompanyReconciliation : OrganizationEntity
{
    public IntercompanyReconciliation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        Guid periodId,
        Guid leftCompanyId,
        Guid rightCompanyId,
        decimal differenceAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        PeriodId = DomainRules.Id(periodId, nameof(periodId));
        LeftCompanyId = DomainRules.Id(leftCompanyId, nameof(leftCompanyId));
        RightCompanyId = DomainRules.Id(rightCompanyId, nameof(rightCompanyId));
        DifferenceAmount = DomainRules.Nonnegative(differenceAmount, nameof(differenceAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid CompanyGroupId { get; }
    public Guid PeriodId { get; }
    public Guid LeftCompanyId { get; }
    public Guid RightCompanyId { get; }
    public decimal DifferenceAmount { get; }
    public string Currency { get; }
}
