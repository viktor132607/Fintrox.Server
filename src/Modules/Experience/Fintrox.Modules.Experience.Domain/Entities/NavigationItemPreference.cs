namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned NavigationItemPreference domain model.</summary>
public sealed class NavigationItemPreference : OrganizationEntity
{
    public NavigationItemPreference(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        Guid navigationItemId,
        int sortOrder,
        bool isPinned)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        NavigationItemId = DomainRules.Id(navigationItemId, nameof(navigationItemId));
        SortOrder = DomainRules.NonnegativeInt(sortOrder, nameof(sortOrder));
        IsPinned = isPinned;
    }

    public Guid UserId { get; }
    public Guid NavigationItemId { get; }
    public int SortOrder { get; }
    public bool IsPinned { get; }
}
