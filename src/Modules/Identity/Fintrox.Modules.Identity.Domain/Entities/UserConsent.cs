namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserConsent domain record; references checked in application layer.</summary>
public sealed class UserConsent : OrganizationEntity
{
    public UserConsent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string consentPurpose,
        string consentStatus,
        DateTimeOffset recordedAtUtc,
        string policyVersion)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        ConsentPurpose = DomainRules.Text(consentPurpose, nameof(consentPurpose));
        ConsentStatus = DomainRules.Text(consentStatus, nameof(consentStatus));
        RecordedAtUtc = DomainRules.Instant(recordedAtUtc, nameof(recordedAtUtc));
        PolicyVersion = DomainRules.Text(policyVersion, nameof(policyVersion));
    }

    public Guid UserId { get; }
    public string ConsentPurpose { get; }
    public string ConsentStatus { get; }
    public DateTimeOffset RecordedAtUtc { get; }
    public string PolicyVersion { get; }
}
