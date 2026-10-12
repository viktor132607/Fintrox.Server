namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserPhoneNumber domain record; references checked in application layer.</summary>
public sealed class UserPhoneNumber : OrganizationEntity
{
    public UserPhoneNumber(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string phoneNumber,
        string countryCode,
        bool isPrimary,
        bool isVerified)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        PhoneNumber = DomainRules.Text(phoneNumber, nameof(phoneNumber));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        IsPrimary = isPrimary;
        IsVerified = isVerified;
    }

    public Guid UserId { get; }
    public string PhoneNumber { get; }
    public string CountryCode { get; }
    public bool IsPrimary { get; }
    public bool IsVerified { get; }
}
