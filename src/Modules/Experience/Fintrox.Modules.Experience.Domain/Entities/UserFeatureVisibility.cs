namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned UserFeatureVisibility domain model.</summary>
public sealed class UserFeatureVisibility : OrganizationEntity
{
    public UserFeatureVisibility(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string capabilityCode,
        bool isVisible,
        DateTimeOffset updatedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        CapabilityCode = DomainRules.Text(capabilityCode, nameof(capabilityCode));
        IsVisible = isVisible;
        UpdatedAtUtc = DomainRules.Instant(updatedAtUtc, nameof(updatedAtUtc));
    }

    public Guid UserId { get; }
    public string CapabilityCode { get; }
    public bool IsVisible { get; }
    public DateTimeOffset UpdatedAtUtc { get; }
}
