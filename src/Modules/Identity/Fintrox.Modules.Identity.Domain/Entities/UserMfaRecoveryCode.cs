namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserMfaRecoveryCode domain record; references checked in application layer.</summary>
public sealed class UserMfaRecoveryCode : OrganizationEntity
{
    public UserMfaRecoveryCode(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string tokenHash,
        DateTimeOffset issuedAtUtc,
        bool isRedeemed)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        TokenHash = DomainRules.Hash(tokenHash, nameof(tokenHash));
        IssuedAtUtc = DomainRules.Instant(issuedAtUtc, nameof(issuedAtUtc));
        IsRedeemed = isRedeemed;
    }

    public Guid UserId { get; }
    public string TokenHash { get; }
    public DateTimeOffset IssuedAtUtc { get; }
    public bool IsRedeemed { get; }
}
