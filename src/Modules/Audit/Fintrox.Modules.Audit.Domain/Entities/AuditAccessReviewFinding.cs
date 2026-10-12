namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditAccessReviewFinding record; cross-module references require application authorization.</summary>
public sealed class AuditAccessReviewFinding : OrganizationEntity
{
    public AuditAccessReviewFinding(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid accessReviewId,
        Guid userId,
        string permissionCode,
        string finding,
        string riskLevel)
        : base(id, organizationId, createdAtUtc)
    {
        AccessReviewId = DomainRules.Id(accessReviewId, nameof(accessReviewId));
        UserId = DomainRules.Id(userId, nameof(userId));
        PermissionCode = DomainRules.Text(permissionCode, nameof(permissionCode));
        Finding = DomainRules.Text(finding, nameof(finding));
        RiskLevel = DomainRules.Text(riskLevel, nameof(riskLevel));
    }

    public Guid AccessReviewId { get; }
    public Guid UserId { get; }
    public string PermissionCode { get; }
    public string Finding { get; }
    public string RiskLevel { get; }
}
