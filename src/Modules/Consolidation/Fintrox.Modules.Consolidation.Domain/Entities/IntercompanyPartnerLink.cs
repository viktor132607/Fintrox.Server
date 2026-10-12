namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned IntercompanyPartnerLink; external references validated by application layer.</summary>
public sealed class IntercompanyPartnerLink : OrganizationEntity
{
    public IntercompanyPartnerLink(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        Guid sourceCompanyId,
        Guid targetCompanyId,
        string partnerCode,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        SourceCompanyId = DomainRules.Id(sourceCompanyId, nameof(sourceCompanyId));
        TargetCompanyId = DomainRules.Id(targetCompanyId, nameof(targetCompanyId));
        PartnerCode = DomainRules.Text(partnerCode, nameof(partnerCode));
        IsActive = isActive;
    }

    public Guid CompanyGroupId { get; }
    public Guid SourceCompanyId { get; }
    public Guid TargetCompanyId { get; }
    public string PartnerCode { get; }
    public bool IsActive { get; }
}
