namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserProfile domain record; references checked in application layer.</summary>
public sealed class UserProfile : OrganizationEntity
{
    public UserProfile(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string displayName,
        string preferredLanguage,
        string timeZoneId,
        bool isPublic)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        DisplayName = DomainRules.Text(displayName, nameof(displayName));
        PreferredLanguage = DomainRules.Text(preferredLanguage, nameof(preferredLanguage));
        TimeZoneId = DomainRules.Text(timeZoneId, nameof(timeZoneId));
        IsPublic = isPublic;
    }

    public Guid UserId { get; }
    public string DisplayName { get; }
    public string PreferredLanguage { get; }
    public string TimeZoneId { get; }
    public bool IsPublic { get; }
}
