namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned ServiceAccountCredential domain record; references checked in application layer.</summary>
public sealed class ServiceAccountCredential : OrganizationEntity
{
    public ServiceAccountCredential(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid serviceAccountId,
        string tokenHash,
        DateTimeOffset issuedAtUtc,
        DateTimeOffset expiresAtUtc,
        bool isRevoked)
        : base(id, organizationId, createdAtUtc)
    {
        ServiceAccountId = DomainRules.Id(serviceAccountId, nameof(serviceAccountId));
        TokenHash = DomainRules.Hash(tokenHash, nameof(tokenHash));
        IssuedAtUtc = DomainRules.Instant(issuedAtUtc, nameof(issuedAtUtc));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
        IsRevoked = isRevoked;
    }

    public Guid ServiceAccountId { get; }
    public string TokenHash { get; }
    public DateTimeOffset IssuedAtUtc { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public bool IsRevoked { get; }
}
