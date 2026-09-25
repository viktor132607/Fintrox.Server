using Fintrox.Domain.Common;

namespace Fintrox.Domain.Tax;

public sealed class VatCode : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private VatCode()
    {
    }

    private VatCode(
        Guid id,
        Guid organizationId,
        string code,
        string name,
        VatCodeKind kind,
        decimal ratePercent,
        DateOnly validFrom,
        DateOnly? validTo,
        bool appliesToSales,
        bool appliesToPurchases,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        Code = NormalizeCode(code);
        Name = NormalizeName(name);
        ValidateRate(kind, ratePercent);
        ValidateValidity(validFrom, validTo);
        ValidateApplicability(appliesToSales, appliesToPurchases);

        Kind = kind;
        RatePercent = ratePercent;
        ValidFrom = validFrom;
        ValidTo = validTo;
        AppliesToSales = appliesToSales;
        AppliesToPurchases = appliesToPurchases;
        IsActive = true;
    }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public VatCodeKind Kind { get; private set; }

    public decimal RatePercent { get; private set; }

    public DateOnly ValidFrom { get; private set; }

    public DateOnly? ValidTo { get; private set; }

    public bool AppliesToSales { get; private set; }

    public bool AppliesToPurchases { get; private set; }

    public bool IsActive { get; private set; }

    public static VatCode Create(
        Guid organizationId,
        string code,
        string name,
        VatCodeKind kind,
        decimal ratePercent,
        DateOnly validFrom,
        DateOnly? validTo,
        bool appliesToSales,
        bool appliesToPurchases,
        DateTimeOffset now)
    {
        return new VatCode(
            Guid.NewGuid(),
            organizationId,
            code,
            name,
            kind,
            ratePercent,
            validFrom,
            validTo,
            appliesToSales,
            appliesToPurchases,
            now);
    }

    public void Update(
        string code,
        string name,
        VatCodeKind kind,
        decimal ratePercent,
        DateOnly validFrom,
        DateOnly? validTo,
        bool appliesToSales,
        bool appliesToPurchases,
        DateTimeOffset now)
    {
        Code = NormalizeCode(code);
        Name = NormalizeName(name);
        ValidateRate(kind, ratePercent);
        ValidateValidity(validFrom, validTo);
        ValidateApplicability(appliesToSales, appliesToPurchases);

        Kind = kind;
        RatePercent = ratePercent;
        ValidFrom = validFrom;
        ValidTo = validTo;
        AppliesToSales = appliesToSales;
        AppliesToPurchases = appliesToPurchases;
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

    public bool IsValidOn(DateOnly date) =>
        ValidFrom <= date &&
        (!ValidTo.HasValue || ValidTo.Value >= date);

    private static string NormalizeCode(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("VAT code is required.", nameof(value));
        }

        if (normalized.Length > 32)
        {
            throw new ArgumentException(
                "VAT code cannot exceed 32 characters.",
                nameof(value));
        }

        return normalized;
    }

    private static string NormalizeName(string value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("VAT code name is required.", nameof(value));
        }

        if (normalized.Length > 160)
        {
            throw new ArgumentException(
                "VAT code name cannot exceed 160 characters.",
                nameof(value));
        }

        return normalized;
    }

    private static void ValidateRate(
        VatCodeKind kind,
        decimal ratePercent)
    {
        if (ratePercent is < 0m or > 100m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ratePercent),
                "VAT rate must be between 0 and 100 percent.");
        }

        if (decimal.Round(ratePercent, 4) != ratePercent)
        {
            throw new ArgumentException(
                "VAT rate supports at most four decimal places.",
                nameof(ratePercent));
        }

        if (kind is VatCodeKind.ZeroRated
            or VatCodeKind.Exempt
            or VatCodeKind.OutOfScope &&
            ratePercent != 0m)
        {
            throw new ArgumentException(
                "Zero-rated, exempt and out-of-scope VAT codes must have a zero rate.",
                nameof(ratePercent));
        }

        if (kind is VatCodeKind.Standard or VatCodeKind.Reduced &&
            ratePercent <= 0m)
        {
            throw new ArgumentException(
                "Standard and reduced VAT codes must have a positive rate.",
                nameof(ratePercent));
        }
    }

    private static void ValidateValidity(
        DateOnly validFrom,
        DateOnly? validTo)
    {
        if (validTo.HasValue && validTo.Value < validFrom)
        {
            throw new ArgumentException(
                "VAT code valid-to date cannot be before valid-from date.",
                nameof(validTo));
        }
    }

    private static void ValidateApplicability(
        bool appliesToSales,
        bool appliesToPurchases)
    {
        if (!appliesToSales && !appliesToPurchases)
        {
            throw new ArgumentException(
                "A VAT code must apply to sales, purchases, or both.");
        }
    }
}
