namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned AccountLockout domain record; references checked in application layer.</summary>
public sealed class AccountLockout : OrganizationEntity
{
    public AccountLockout(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string lockoutReason,
        DateTimeOffset lockedAtUtc,
        DateTimeOffset unlockAfterUtc,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        LockoutReason = DomainRules.Text(lockoutReason, nameof(lockoutReason));
        LockedAtUtc = DomainRules.Instant(lockedAtUtc, nameof(lockedAtUtc));
        UnlockAfterUtc = DomainRules.Instant(unlockAfterUtc, nameof(unlockAfterUtc));
        IsActive = isActive;
    }

    public Guid UserId { get; }
    public string LockoutReason { get; }
    public DateTimeOffset LockedAtUtc { get; }
    public DateTimeOffset UnlockAfterUtc { get; }
    public bool IsActive { get; }
}
