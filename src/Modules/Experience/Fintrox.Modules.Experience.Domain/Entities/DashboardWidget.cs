namespace Fintrox.Modules.Experience.Domain.Entities;

public sealed class DashboardWidget : OrganizationEntity
{
    public DashboardWidget(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid profileId,
        string widgetCode,
        int position)
        : base(id, organizationId, createdAtUtc)
    {
        ProfileId = DomainRules.Id(profileId, nameof(profileId));
        WidgetCode = DomainRules.Text(widgetCode, nameof(widgetCode));
        Position = DomainRules.NonnegativeInt(position, nameof(position));
    }

    public Guid ProfileId { get; }
    public string WidgetCode { get; }
    public int Position { get; }
}
