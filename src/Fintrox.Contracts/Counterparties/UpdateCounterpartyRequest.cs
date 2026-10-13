using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Counterparties;

public sealed record UpdateCounterpartyRequest(
    [param: Required, MaxLength(32)] string Code,
    [param: Required, MaxLength(200)] string Name,
    [param: MaxLength(200)] string? LegalName,
    [param: Required, StringLength(2, MinimumLength = 2)] string CountryCode,
    [param: MaxLength(64)] string? RegistrationNumber,
    [param: MaxLength(64)] string? VatNumber,
    bool IsCustomer,
    bool IsSupplier,
    [param: Range(0, 3650)] int PaymentTermDays,
    [param: MaxLength(160)] string? ContactPerson,
    [param: EmailAddress, MaxLength(254)] string? Email,
    [param: MaxLength(64)] string? Phone,
    [param: MaxLength(240)] string? AddressLine1,
    [param: MaxLength(240)] string? AddressLine2,
    [param: MaxLength(120)] string? City,
    [param: MaxLength(32)] string? PostalCode,
    [param: Url, MaxLength(255)] string? Website,
    [param: MaxLength(1000)] string? Notes);
