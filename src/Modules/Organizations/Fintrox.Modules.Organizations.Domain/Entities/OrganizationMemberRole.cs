namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationMemberRole record.</summary>
public sealed class OrganizationMemberRole : OrganizationEntity
{
    public OrganizationMemberRole(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid memberUserId,
        string roleCode,
        DateOnly validFrom,
        bool isOwner)
        : base(id, organizationId, createdAtUtc)
    {
        MemberUserId = DomainRules.Id(memberUserId, nameof(memberUserId));
        RoleCode = DomainRules.Text(roleCode, nameof(roleCode));
        ValidFrom = DomainRules.Date(validFrom, nameof(validFrom));
        IsOwner = isOwner;
    }

    public Guid MemberUserId { get; }
    public string RoleCode { get; }
    public DateOnly ValidFrom { get; }
    public bool IsOwner { get; }
}
