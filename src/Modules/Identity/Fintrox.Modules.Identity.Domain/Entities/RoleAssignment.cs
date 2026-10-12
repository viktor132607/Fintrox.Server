namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned RoleAssignment domain record; references checked in application layer.</summary>
public sealed class RoleAssignment : OrganizationEntity
{
    public RoleAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        Guid roleDefinitionId,
        string scopeCode,
        Guid grantedByUserId,
        DateTimeOffset grantedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        RoleDefinitionId = DomainRules.Id(roleDefinitionId, nameof(roleDefinitionId));
        ScopeCode = DomainRules.Text(scopeCode, nameof(scopeCode));
        GrantedByUserId = DomainRules.Id(grantedByUserId, nameof(grantedByUserId));
        GrantedAtUtc = DomainRules.Instant(grantedAtUtc, nameof(grantedAtUtc));
    }

    public Guid UserId { get; }
    public Guid RoleDefinitionId { get; }
    public string ScopeCode { get; }
    public Guid GrantedByUserId { get; }
    public DateTimeOffset GrantedAtUtc { get; }
}
