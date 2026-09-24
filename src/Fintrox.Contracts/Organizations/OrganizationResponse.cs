namespace Fintrox.Contracts.Organizations;

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    string? LegalName,
    string Slug,
    string CountryCode,
    string BaseCurrencyCode,
    string TimeZoneId,
    string? RegistrationNumber,
    string? VatNumber,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
