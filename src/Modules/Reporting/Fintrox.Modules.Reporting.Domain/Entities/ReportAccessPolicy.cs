namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportAccessPolicy model. References require application validation.</summary>
public sealed class ReportAccessPolicy : OrganizationEntity
{
    public ReportAccessPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        string roleCode,
        string accessLevel,
        bool isAllowed)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        RoleCode = DomainRules.Text(roleCode, nameof(roleCode));
        AccessLevel = DomainRules.Text(accessLevel, nameof(accessLevel));
        IsAllowed = isAllowed;
    }

    public Guid ReportDefinitionId { get; }
    public string RoleCode { get; }
    public string AccessLevel { get; }
    public bool IsAllowed { get; }
}
