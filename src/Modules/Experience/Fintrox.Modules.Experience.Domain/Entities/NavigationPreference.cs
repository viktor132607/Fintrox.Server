namespace Fintrox.Modules.Experience.Domain.Entities;

public sealed class NavigationPreference : OrganizationEntity
{
    public NavigationPreference(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string capabilityCode,
        int position,
        bool visible)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        CapabilityCode = DomainRules.Text(capabilityCode, nameof(capabilityCode));
        Position = DomainRules.NonnegativeInt(position, nameof(position));
        Visible = visible;
    }

    public Guid UserId { get; }
    public string CapabilityCode { get; }
    public int Position { get; }
    public bool Visible { get; }
}
