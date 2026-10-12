namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned RoleScopeAssignment domain record; references checked in application layer.</summary>
public sealed class RoleScopeAssignment : OrganizationEntity
{
    public RoleScopeAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid roleDefinitionId,
        Guid resourceId,
        string resourceType,
        DateOnly validFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        RoleDefinitionId = DomainRules.Id(roleDefinitionId, nameof(roleDefinitionId));
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        ValidFrom = DomainRules.Date(validFrom, nameof(validFrom));
        IsActive = isActive;
    }

    public Guid RoleDefinitionId { get; }
    public Guid ResourceId { get; }
    public string ResourceType { get; }
    public DateOnly ValidFrom { get; }
    public bool IsActive { get; }
}
