namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned WidgetCatalogEntry domain model.</summary>
public sealed class WidgetCatalogEntry : OrganizationEntity
{
    public WidgetCatalogEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string widgetCode,
        string moduleCode,
        string displayName,
        string minimumAccessLevel,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        WidgetCode = DomainRules.Text(widgetCode, nameof(widgetCode));
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
        DisplayName = DomainRules.Text(displayName, nameof(displayName));
        MinimumAccessLevel = DomainRules.Text(minimumAccessLevel, nameof(minimumAccessLevel));
        IsEnabled = isEnabled;
    }

    public string WidgetCode { get; }
    public string ModuleCode { get; }
    public string DisplayName { get; }
    public string MinimumAccessLevel { get; }
    public bool IsEnabled { get; }
}
