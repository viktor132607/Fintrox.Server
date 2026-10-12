namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserMfaFactor domain record; references checked in application layer.</summary>
public sealed class UserMfaFactor : OrganizationEntity
{
    public UserMfaFactor(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string factorType,
        string providerCode,
        DateTimeOffset enrolledAtUtc,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        FactorType = DomainRules.Text(factorType, nameof(factorType));
        ProviderCode = DomainRules.Text(providerCode, nameof(providerCode));
        EnrolledAtUtc = DomainRules.Instant(enrolledAtUtc, nameof(enrolledAtUtc));
        IsEnabled = isEnabled;
    }

    public Guid UserId { get; }
    public string FactorType { get; }
    public string ProviderCode { get; }
    public DateTimeOffset EnrolledAtUtc { get; }
    public bool IsEnabled { get; }
}
