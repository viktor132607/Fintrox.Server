namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourcePool model; no cross-module navigations.</summary>
public sealed class ResourcePool : OrganizationEntity
{
    public ResourcePool(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string poolCode,
        string name,
        Guid managerUserId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        PoolCode = DomainRules.Text(poolCode, nameof(poolCode));
        Name = DomainRules.Text(name, nameof(name));
        ManagerUserId = DomainRules.Id(managerUserId, nameof(managerUserId));
        IsActive = isActive;
    }

    public string PoolCode { get; }
    public string Name { get; }
    public Guid ManagerUserId { get; }
    public bool IsActive { get; }
}
