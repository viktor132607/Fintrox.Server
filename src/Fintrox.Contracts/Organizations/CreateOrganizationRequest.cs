using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Organizations;

public sealed record CreateOrganizationRequest(
    [param: Required, MaxLength(160)] string Name,
    [param: MaxLength(200)] string? LegalName,
    [param: Required, MaxLength(80), RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")] string Slug,
    [param: Required, RegularExpression("^[A-Za-z]{2}$")] string CountryCode,
    [param: Required, RegularExpression("^[A-Za-z]{3}$")] string BaseCurrencyCode,
    [param: Required, MaxLength(64)] string TimeZoneId,
    [param: MaxLength(64)] string? RegistrationNumber,
    [param: MaxLength(64)] string? VatNumber);
