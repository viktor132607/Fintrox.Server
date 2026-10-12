namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectPortfolio; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectPortfolio : OrganizationEntity
{
    public ProjectPortfolio(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        Guid ownerUserId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public Guid OwnerUserId { get; }
    public bool IsActive { get; }
}
