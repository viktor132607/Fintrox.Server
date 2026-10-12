namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserEmailAddress domain record; references checked in application layer.</summary>
public sealed class UserEmailAddress : OrganizationEntity
{
    public UserEmailAddress(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string email,
        bool isPrimary,
        bool isVerified,
        DateTimeOffset verifiedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        Email = DomainRules.Text(email, nameof(email));
        IsPrimary = isPrimary;
        IsVerified = isVerified;
        VerifiedAtUtc = DomainRules.Instant(verifiedAtUtc, nameof(verifiedAtUtc));
    }

    public Guid UserId { get; }
    public string Email { get; }
    public bool IsPrimary { get; }
    public bool IsVerified { get; }
    public DateTimeOffset VerifiedAtUtc { get; }
}
