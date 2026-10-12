namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned PermissionPolicyRule domain record; references checked in application layer.</summary>
public sealed class PermissionPolicyRule : OrganizationEntity
{
    public PermissionPolicyRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid policyId,
        Guid permissionDefinitionId,
        string effect,
        string conditionJson)
        : base(id, organizationId, createdAtUtc)
    {
        PolicyId = DomainRules.Id(policyId, nameof(policyId));
        PermissionDefinitionId = DomainRules.Id(permissionDefinitionId, nameof(permissionDefinitionId));
        Effect = DomainRules.Text(effect, nameof(effect));
        ConditionJson = DomainRules.Json(conditionJson, nameof(conditionJson));
    }

    public Guid PolicyId { get; }
    public Guid PermissionDefinitionId { get; }
    public string Effect { get; }
    public string ConditionJson { get; }
}
