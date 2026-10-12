namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditPolicyAssignment record; cross-module references require application authorization.</summary>
public sealed class AuditPolicyAssignment : OrganizationEntity
{
    public AuditPolicyAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid retentionPolicyId,
        string resourceType,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        RetentionPolicyId = DomainRules.Id(retentionPolicyId, nameof(retentionPolicyId));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public Guid RetentionPolicyId { get; }
    public string ResourceType { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
