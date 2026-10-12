namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned GroupAccountingPolicy; external references validated by application layer.</summary>
public sealed class GroupAccountingPolicy : OrganizationEntity
{
    public GroupAccountingPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        string policyCode,
        DateOnly effectiveFrom,
        string policyJson,
        int versionNumber)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        PolicyCode = DomainRules.Text(policyCode, nameof(policyCode));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        PolicyJson = DomainRules.Json(policyJson, nameof(policyJson));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
    }

    public Guid CompanyGroupId { get; }
    public string PolicyCode { get; }
    public DateOnly EffectiveFrom { get; }
    public string PolicyJson { get; }
    public int VersionNumber { get; }
}
