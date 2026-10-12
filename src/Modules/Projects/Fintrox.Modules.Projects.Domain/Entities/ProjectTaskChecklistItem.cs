namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectTaskChecklistItem; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectTaskChecklistItem : OrganizationEntity
{
    public ProjectTaskChecklistItem(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectTaskId,
        string itemDescription,
        int sortOrder,
        bool isComplete)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectTaskId = DomainRules.Id(projectTaskId, nameof(projectTaskId));
        ItemDescription = DomainRules.Text(itemDescription, nameof(itemDescription));
        SortOrder = DomainRules.NonnegativeInt(sortOrder, nameof(sortOrder));
        IsComplete = isComplete;
    }

    public Guid ProjectTaskId { get; }
    public string ItemDescription { get; }
    public int SortOrder { get; }
    public bool IsComplete { get; }
}
