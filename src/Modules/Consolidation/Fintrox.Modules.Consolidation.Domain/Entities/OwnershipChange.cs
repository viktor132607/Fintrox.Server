namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned OwnershipChange; external references validated by application layer.</summary>
public sealed class OwnershipChange : OrganizationEntity
{
    public OwnershipChange(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        Guid subsidiaryCompanyId,
        DateOnly effectiveOn,
        decimal oldPercent,
        decimal newPercent)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        SubsidiaryCompanyId = DomainRules.Id(subsidiaryCompanyId, nameof(subsidiaryCompanyId));
        EffectiveOn = DomainRules.Date(effectiveOn, nameof(effectiveOn));
        OldPercent = DomainRules.Percent(oldPercent, nameof(oldPercent));
        NewPercent = DomainRules.Percent(newPercent, nameof(newPercent));
    }

    public Guid CompanyGroupId { get; }
    public Guid SubsidiaryCompanyId { get; }
    public DateOnly EffectiveOn { get; }
    public decimal OldPercent { get; }
    public decimal NewPercent { get; }
}
