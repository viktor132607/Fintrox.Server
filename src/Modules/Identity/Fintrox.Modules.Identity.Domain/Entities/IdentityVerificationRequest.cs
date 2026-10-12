namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned IdentityVerificationRequest domain record; references checked in application layer.</summary>
public sealed class IdentityVerificationRequest : OrganizationEntity
{
    public IdentityVerificationRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string verificationType,
        Guid challengeId,
        DateTimeOffset requestedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        VerificationType = DomainRules.Text(verificationType, nameof(verificationType));
        ChallengeId = DomainRules.Id(challengeId, nameof(challengeId));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid UserId { get; }
    public string VerificationType { get; }
    public Guid ChallengeId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string Status { get; }
}
