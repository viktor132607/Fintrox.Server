using Fintrox.Domain.Common;

namespace Fintrox.Domain.Partners;

public sealed class Counterparty : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private Counterparty()
    {
    }

    private Counterparty(
        Guid id,
        Guid organizationId,
        string code,
        string name,
        string? legalName,
        string countryCode,
        string? registrationNumber,
        string? vatNumber,
        bool isCustomer,
        bool isSupplier,
        int paymentTermDays,
        string? contactPerson,
        string? email,
        string? phone,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? postalCode,
        string? website,
        string? notes,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        Code = NormalizeRequired(code, nameof(code), 32).ToUpperInvariant();
        Name = NormalizeRequired(name, nameof(name), 200);
        LegalName = NormalizeOptional(legalName, 200);
        CountryCode = NormalizeCountryCode(countryCode);
        RegistrationNumber = NormalizeIdentifier(registrationNumber, 64);
        VatNumber = NormalizeIdentifier(vatNumber, 64);
        ValidateRoles(isCustomer, isSupplier);
        ValidatePaymentTermDays(paymentTermDays);
        IsCustomer = isCustomer;
        IsSupplier = isSupplier;
        PaymentTermDays = paymentTermDays;
        ContactPerson = NormalizeOptional(contactPerson, 160);
        Email = NormalizeEmail(email);
        Phone = NormalizeOptional(phone, 64);
        AddressLine1 = NormalizeOptional(addressLine1, 240);
        AddressLine2 = NormalizeOptional(addressLine2, 240);
        City = NormalizeOptional(city, 120);
        PostalCode = NormalizeOptional(postalCode, 32);
        Website = NormalizeOptional(website, 255);
        Notes = NormalizeOptional(notes, 1000);
        IsActive = true;
    }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? LegalName { get; private set; }

    public string CountryCode { get; private set; } = null!;

    public string? RegistrationNumber { get; private set; }

    public string? VatNumber { get; private set; }

    public bool IsCustomer { get; private set; }

    public bool IsSupplier { get; private set; }

    public int PaymentTermDays { get; private set; }

    public string? ContactPerson { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? AddressLine1 { get; private set; }

    public string? AddressLine2 { get; private set; }

    public string? City { get; private set; }

    public string? PostalCode { get; private set; }

    public string? Website { get; private set; }

    public string? Notes { get; private set; }

    public bool IsActive { get; private set; }

    public static Counterparty Create(
        Guid organizationId,
        string code,
        string name,
        string? legalName,
        string countryCode,
        string? registrationNumber,
        string? vatNumber,
        bool isCustomer,
        bool isSupplier,
        int paymentTermDays,
        string? contactPerson,
        string? email,
        string? phone,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? postalCode,
        string? website,
        string? notes,
        DateTimeOffset now)
    {
        return new Counterparty(
            Guid.NewGuid(),
            organizationId,
            code,
            name,
            legalName,
            countryCode,
            registrationNumber,
            vatNumber,
            isCustomer,
            isSupplier,
            paymentTermDays,
            contactPerson,
            email,
            phone,
            addressLine1,
            addressLine2,
            city,
            postalCode,
            website,
            notes,
            now);
    }

    public void Update(
        string code,
        string name,
        string? legalName,
        string countryCode,
        string? registrationNumber,
        string? vatNumber,
        bool isCustomer,
        bool isSupplier,
        int paymentTermDays,
        string? contactPerson,
        string? email,
        string? phone,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? postalCode,
        string? website,
        string? notes,
        DateTimeOffset now)
    {
        Code = NormalizeRequired(code, nameof(code), 32).ToUpperInvariant();
        Name = NormalizeRequired(name, nameof(name), 200);
        LegalName = NormalizeOptional(legalName, 200);
        CountryCode = NormalizeCountryCode(countryCode);
        RegistrationNumber = NormalizeIdentifier(registrationNumber, 64);
        VatNumber = NormalizeIdentifier(vatNumber, 64);
        ValidateRoles(isCustomer, isSupplier);
        ValidatePaymentTermDays(paymentTermDays);
        IsCustomer = isCustomer;
        IsSupplier = isSupplier;
        PaymentTermDays = paymentTermDays;
        ContactPerson = NormalizeOptional(contactPerson, 160);
        Email = NormalizeEmail(email);
        Phone = NormalizeOptional(phone, 64);
        AddressLine1 = NormalizeOptional(addressLine1, 240);
        AddressLine2 = NormalizeOptional(addressLine2, 240);
        City = NormalizeOptional(city, 120);
        PostalCode = NormalizeOptional(postalCode, 32);
        Website = NormalizeOptional(website, 255);
        Notes = NormalizeOptional(notes, 1000);
        Touch(now);
    }

    public void Activate(DateTimeOffset now)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(now);
    }

    public void Deactivate(DateTimeOffset now)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(now);
    }

    private static void ValidateRoles(bool isCustomer, bool isSupplier)
    {
        if (!isCustomer && !isSupplier)
        {
            throw new ArgumentException(
                "A counterparty must be a customer, a supplier, or both.");
        }
    }

    private static void ValidatePaymentTermDays(int paymentTermDays)
    {
        if (paymentTermDays is < 0 or > 3650)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paymentTermDays),
                "Payment term days must be between 0 and 3650.");
        }
    }

    private static string NormalizeCountryCode(string value)
    {
        var normalized = NormalizeRequired(
            value,
            nameof(value),
            2).ToUpperInvariant();

        if (normalized.Length != 2 ||
            !normalized.All(char.IsAsciiLetter))
        {
            throw new ArgumentException(
                "Country code must contain exactly two ASCII letters.",
                nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeIdentifier(
        string? value,
        int maxLength)
    {
        var normalized = NormalizeOptional(value, maxLength);
        return normalized?.ToUpperInvariant();
    }

    private static string? NormalizeEmail(string? value)
    {
        var normalized = NormalizeOptional(value, 254);
        return normalized?.ToLowerInvariant();
    }

    private static string NormalizeRequired(
        string value,
        string parameterName,
        int maxLength)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "Value is required.",
                parameterName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                nameof(value));
        }

        return normalized;
    }
}
