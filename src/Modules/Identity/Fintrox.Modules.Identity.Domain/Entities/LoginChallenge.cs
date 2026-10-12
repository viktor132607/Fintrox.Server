namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned LoginChallenge domain record; references checked in application layer.</summary>
public sealed class LoginChallenge : OrganizationEntity
{
    public LoginChallenge(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string challengeType,
        string tokenHash,
        DateTimeOffset issuedAtUtc,
        DateTimeOffset expiresAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        ChallengeType = DomainRules.Text(challengeType, nameof(challengeType));
        TokenHash = DomainRules.Hash(tokenHash, nameof(tokenHash));
        IssuedAtUtc = DomainRules.Instant(issuedAtUtc, nameof(issuedAtUtc));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
    }

    public Guid UserId { get; }
    public string ChallengeType { get; }
    public string TokenHash { get; }
    public DateTimeOffset IssuedAtUtc { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
}
