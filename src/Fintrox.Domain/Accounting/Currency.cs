using Fintrox.Domain.Common;

namespace Fintrox.Domain.Accounting;

public sealed class Currency : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private Currency()
    {
    }

    private Currency(
        Guid id,
        Guid organizationId,
        string code,
        string name,
        string? symbol,
        int decimalPlaces,
        bool isBaseCurrency,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        Code = NormalizeCode(code);
        Name = NormalizeName(name);
        Symbol = NormalizeSymbol(symbol);
        ValidateDecimalPlaces(decimalPlaces);

        DecimalPlaces = decimalPlaces;
        IsBaseCurrency = isBaseCurrency;
        IsActive = true;
    }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Symbol { get; private set; }

    public int DecimalPlaces { get; private set; }

    public bool IsBaseCurrency { get; private set; }

    public bool IsActive { get; private set; }

    public static Currency Create(
        Guid organizationId,
        string code,
        string name,
        string? symbol,
        int decimalPlaces,
        bool isBaseCurrency,
        DateTimeOffset now)
    {
        return new Currency(
            Guid.NewGuid(),
            organizationId,
            code,
            name,
            symbol,
            decimalPlaces,
            isBaseCurrency,
            now);
    }

    public void Update(
        string code,
        string name,
        string? symbol,
        int decimalPlaces,
        DateTimeOffset now)
    {
        Code = NormalizeCode(code);
        Name = NormalizeName(name);
        Symbol = NormalizeSymbol(symbol);
        ValidateDecimalPlaces(decimalPlaces);

        DecimalPlaces = decimalPlaces;
        Touch(now);
    }

    public void SetAsBase(DateTimeOffset now)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                "An inactive currency cannot be the base currency.");
        }

        if (IsBaseCurrency)
        {
            return;
        }

        IsBaseCurrency = true;
        Touch(now);
    }

    public void ClearBase(DateTimeOffset now)
    {
        if (!IsBaseCurrency)
        {
            return;
        }

        IsBaseCurrency = false;
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
        if (IsBaseCurrency)
        {
            throw new InvalidOperationException(
                "The base currency cannot be deactivated.");
        }

        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(now);
    }

    private static string NormalizeCode(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant();

        if (normalized is null ||
            normalized.Length != 3 ||
            !normalized.All(char.IsAsciiLetter))
        {
            throw new ArgumentException(
                "Currency code must contain exactly three ASCII letters.",
                nameof(value));
        }

        return normalized;
    }

    private static string NormalizeName(string value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Currency name is required.", nameof(value));
        }

        if (normalized.Length > 120)
        {
            throw new ArgumentException(
                "Currency name cannot exceed 120 characters.",
                nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeSymbol(string? value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > 12)
        {
            throw new ArgumentException(
                "Currency symbol cannot exceed 12 characters.",
                nameof(value));
        }

        return normalized;
    }

    private static void ValidateDecimalPlaces(int decimalPlaces)
    {
        if (decimalPlaces is < 0 or > 4)
        {
            throw new ArgumentOutOfRangeException(
                nameof(decimalPlaces),
                "Currency decimal places must be between 0 and 4.");
        }
    }
}
