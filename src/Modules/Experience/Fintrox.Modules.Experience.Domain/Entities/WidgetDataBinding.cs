namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned WidgetDataBinding domain model.</summary>
public sealed class WidgetDataBinding : OrganizationEntity
{
    public WidgetDataBinding(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid widgetCatalogEntryId,
        string sourceCode,
        string queryKey,
        int refreshIntervalSeconds)
        : base(id, organizationId, createdAtUtc)
    {
        WidgetCatalogEntryId = DomainRules.Id(widgetCatalogEntryId, nameof(widgetCatalogEntryId));
        SourceCode = DomainRules.Text(sourceCode, nameof(sourceCode));
        QueryKey = DomainRules.Text(queryKey, nameof(queryKey));
        RefreshIntervalSeconds = DomainRules.PositiveInt(refreshIntervalSeconds, nameof(refreshIntervalSeconds));
    }

    public Guid WidgetCatalogEntryId { get; }
    public string SourceCode { get; }
    public string QueryKey { get; }
    public int RefreshIntervalSeconds { get; }
}
