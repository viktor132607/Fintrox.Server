namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned UserLayoutSection domain model.</summary>
public sealed class UserLayoutSection : OrganizationEntity
{
    public UserLayoutSection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userLayoutId,
        string sectionCode,
        int position,
        bool isVisible)
        : base(id, organizationId, createdAtUtc)
    {
        UserLayoutId = DomainRules.Id(userLayoutId, nameof(userLayoutId));
        SectionCode = DomainRules.Text(sectionCode, nameof(sectionCode));
        Position = DomainRules.NonnegativeInt(position, nameof(position));
        IsVisible = isVisible;
    }

    public Guid UserLayoutId { get; }
    public string SectionCode { get; }
    public int Position { get; }
    public bool IsVisible { get; }
}
