namespace Fintrox.Modules.Consolidation.Domain.Entities;

public sealed class GroupCompany : OrganizationEntity
{
    public GroupCompany(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        Guid memberOrganizationId,
        decimal ownershipPercent)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        MemberOrganizationId = DomainRules.Id(memberOrganizationId, nameof(memberOrganizationId));
        OwnershipPercent = DomainRules.Percent(ownershipPercent, nameof(ownershipPercent));
    }

    public Guid CompanyGroupId { get; }
    public Guid MemberOrganizationId { get; }
    public decimal OwnershipPercent { get; }
}
