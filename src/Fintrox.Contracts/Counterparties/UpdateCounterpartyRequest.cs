using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Counterparties;

public sealed record UpdateCounterpartyRequest(
    [property: Required, MaxLength(32)] string Code,
    [property: Required, MaxLength(200)] string Name,
    [property: MaxLength(200)] string? LegalName,
    [property: Required, StringLength(2, MinimumLength = 2)] string CountryCode,
    [property: MaxLength(64)] string? RegistrationNumber,
    [property: MaxLength(64)] string? VatNumber,
    bool IsCustomer,
    bool IsSupplier,
    [property: Range(0, 3650)] int PaymentTermDays,
    [property: MaxLength(160)] string? ContactPerson,
    [property: EmailAddress, MaxLength(254)] string? Email,
    [property: MaxLength(64)] string? Phone,
    [property: MaxLength(240)] string? AddressLine1,
    [property: MaxLength(240)] string? AddressLine2,
    [property: MaxLength(120)] string? City,
    [property: MaxLength(32)] string? PostalCode,
    [property: Url, MaxLength(255)] string? Website,
    [property: MaxLength(1000)] string? Notes);
