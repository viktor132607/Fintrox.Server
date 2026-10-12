namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned UserTableColumnPreference domain model.</summary>
public sealed class UserTableColumnPreference : OrganizationEntity
{
    public UserTableColumnPreference(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string tableCode,
        string columnCode,
        int position,
        bool isVisible)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        TableCode = DomainRules.Text(tableCode, nameof(tableCode));
        ColumnCode = DomainRules.Text(columnCode, nameof(columnCode));
        Position = DomainRules.NonnegativeInt(position, nameof(position));
        IsVisible = isVisible;
    }

    public Guid UserId { get; }
    public string TableCode { get; }
    public string ColumnCode { get; }
    public int Position { get; }
    public bool IsVisible { get; }
}
