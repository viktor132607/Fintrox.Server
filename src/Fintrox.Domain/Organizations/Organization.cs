using System.Text.RegularExpressions;
using Fintrox.Domain.Common;

namespace Fintrox.Domain.Organizations;

public sealed partial class Organization : AuditableEntity, IAggregateRoot
{
    private Organization()
    {
    }

    private Organization(
        Guid id,
        string name,
        string? legalName,
        string slug,
        string countryCode,
        string baseCurrencyCode,
        string timeZoneId,
        string? registrationNumber,
        string? vatNumber,
        DateTimeOffset now) : base(id, now)
    {
        Name = name;
        LegalName = legalName;
        Slug = slug;
        CountryCode = countryCode;
        BaseCurrencyCode = baseCurrencyCode;
        TimeZoneId = timeZoneId;
        RegistrationNumber = registrationNumber;
        VatNumber = vatNumber;
        IsActive = true;
    }

    public string Name { get; private set; } = null!;

    public string? LegalName { get; private set; }

    public string Slug { get; private set; } = null!;

    public string CountryCode { get; private set; } = null!;

    public string BaseCurrencyCode { get; private set; } = null!;

    public string TimeZoneId { get; private set; } = null!;

    public string? RegistrationNumber { get; private set; }

    public string? VatNumber { get; private set; }

    public bool IsActive { get; private set; }

    public static Organization Create(
        string name,
        string? legalName,
        string slug,
        string countryCode,
        string baseCurrencyCode,
        string timeZoneId,
        string? registrationNumber,
        string? vatNumber,
        DateTimeOffset now)
    {
        return new Organization(
            Guid.NewGuid(),
            NormalizeRequired(name, nameof(name), 160),
            NormalizeOptional(legalName, 200),
            NormalizeSlug(slug),
            NormalizeCode(countryCode, nameof(countryCode), 2),
            NormalizeCode(baseCurrencyCode, nameof(baseCurrencyCode), 3),
            NormalizeRequired(timeZoneId, nameof(timeZoneId), 64),
            NormalizeOptional(registrationNumber, 64),
            NormalizeOptional(vatNumber, 64),
            now);
    }

    public void Update(
        string name,
        string? legalName,
        string countryCode,
        string baseCurrencyCode,
        string timeZoneId,
        string? registrationNumber,
        string? vatNumber,
        DateTimeOffset now)
    {
        Name = NormalizeRequired(name, nameof(name), 160);
        LegalName = NormalizeOptional(legalName, 200);
        CountryCode = NormalizeCode(countryCode, nameof(countryCode), 2);
        BaseCurrencyCode = NormalizeCode(baseCurrencyCode, nameof(baseCurrencyCode), 3);
        TimeZoneId = NormalizeRequired(timeZoneId, nameof(timeZoneId), 64);
        RegistrationNumber = NormalizeOptional(registrationNumber, 64);
        VatNumber = NormalizeOptional(vatNumber, 64);
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

    public void Activate(DateTimeOffset now)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(now);
    }

    private static string NormalizeRequired(
        string value,
        string parameterName,
        int maxLength)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
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

    private static string NormalizeCode(
        string value,
        string parameterName,
        int length)
    {
        var normalized = NormalizeRequired(
            value,
            parameterName,
            length).ToUpperInvariant();

        if (normalized.Length != length || !normalized.All(char.IsAsciiLetter))
        {
            throw new ArgumentException(
                $"Value must contain exactly {length} ASCII letters.",
                parameterName);
        }

        return normalized;
    }

    private static string NormalizeSlug(string value)
    {
        var normalized = NormalizeRequired(
            value,
            nameof(value),
            80).ToLowerInvariant();

        if (!SlugRegex().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Slug must contain lowercase letters, numbers and single hyphens only.",
                nameof(value));
        }

        return normalized;
    }

    [GeneratedRegex(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.CultureInvariant)]
    private static partial Regex SlugRegex();
}
