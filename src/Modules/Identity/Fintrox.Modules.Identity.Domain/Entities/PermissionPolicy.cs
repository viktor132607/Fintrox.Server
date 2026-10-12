namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned PermissionPolicy domain record; references checked in application layer.</summary>
public sealed class PermissionPolicy : OrganizationEntity
{
    public PermissionPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string policyCode,
        string name,
        string resourceType,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        PolicyCode = DomainRules.Text(policyCode, nameof(policyCode));
        Name = DomainRules.Text(name, nameof(name));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        IsActive = isActive;
    }

    public string PolicyCode { get; }
    public string Name { get; }
    public string ResourceType { get; }
    public bool IsActive { get; }
}
