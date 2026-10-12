namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserPasswordHistory domain record; references checked in application layer.</summary>
public sealed class UserPasswordHistory : OrganizationEntity
{
    public UserPasswordHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string passwordHash,
        DateTimeOffset changedAtUtc,
        string changeReason)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        PasswordHash = DomainRules.Hash(passwordHash, nameof(passwordHash));
        ChangedAtUtc = DomainRules.Instant(changedAtUtc, nameof(changedAtUtc));
        ChangeReason = DomainRules.Text(changeReason, nameof(changeReason));
    }

    public Guid UserId { get; }
    public string PasswordHash { get; }
    public DateTimeOffset ChangedAtUtc { get; }
    public string ChangeReason { get; }
}
