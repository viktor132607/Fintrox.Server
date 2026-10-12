namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned UserLayoutWidget domain model.</summary>
public sealed class UserLayoutWidget : OrganizationEntity
{
    public UserLayoutWidget(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userLayoutId,
        Guid dashboardWidgetId,
        int position,
        string configurationJson)
        : base(id, organizationId, createdAtUtc)
    {
        UserLayoutId = DomainRules.Id(userLayoutId, nameof(userLayoutId));
        DashboardWidgetId = DomainRules.Id(dashboardWidgetId, nameof(dashboardWidgetId));
        Position = DomainRules.NonnegativeInt(position, nameof(position));
        ConfigurationJson = DomainRules.Json(configurationJson, nameof(configurationJson));
    }

    public Guid UserLayoutId { get; }
    public Guid DashboardWidgetId { get; }
    public int Position { get; }
    public string ConfigurationJson { get; }
}
