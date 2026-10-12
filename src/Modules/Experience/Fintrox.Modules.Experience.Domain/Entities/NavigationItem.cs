namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned NavigationItem domain model.</summary>
public sealed class NavigationItem : OrganizationEntity
{
    public NavigationItem(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string moduleCode,
        string route,
        string label,
        int sortOrder,
        bool isVisible)
        : base(id, organizationId, createdAtUtc)
    {
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
        Route = DomainRules.Text(route, nameof(route));
        Label = DomainRules.Text(label, nameof(label));
        SortOrder = DomainRules.NonnegativeInt(sortOrder, nameof(sortOrder));
        IsVisible = isVisible;
    }

    public string ModuleCode { get; }
    public string Route { get; }
    public string Label { get; }
    public int SortOrder { get; }
    public bool IsVisible { get; }
}
