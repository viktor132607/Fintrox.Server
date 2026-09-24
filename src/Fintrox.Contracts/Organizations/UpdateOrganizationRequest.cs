using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Organizations;

public sealed record UpdateOrganizationRequest(
    [property: Required, MaxLength(160)] string Name,
    [property: MaxLength(200)] string? LegalName,
    [property: Required, RegularExpression("^[A-Za-z]{2}$")] string CountryCode,
    [property: Required, RegularExpression("^[A-Za-z]{3}$")] string BaseCurrencyCode,
    [property: Required, MaxLength(64)] string TimeZoneId,
    [property: MaxLength(64)] string? RegistrationNumber,
    [property: MaxLength(64)] string? VatNumber);
