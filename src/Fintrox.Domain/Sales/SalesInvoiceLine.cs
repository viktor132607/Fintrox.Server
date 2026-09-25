using Fintrox.Domain.Common;

namespace Fintrox.Domain.Sales;

public sealed class SalesInvoiceLine : OrganizationScopedAuditableEntity
{
    private SalesInvoiceLine()
    {
    }

    private SalesInvoiceLine(
        Guid id,
        Guid organizationId,
        Guid salesInvoiceId,
        int lineNumber,
        string? itemCode,
        string description,
        decimal quantity,
        string? unitOfMeasure,
        decimal unitPrice,
        decimal discountPercent,
        Guid vatCodeId,
        string vatCode,
        decimal vatRatePercent,
        decimal netAmount,
        decimal vatAmount,
        decimal grossAmount,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        ValidateIds(salesInvoiceId, vatCodeId);
        ValidateLineNumber(lineNumber);
        ValidateCommercialValues(
            quantity,
            unitPrice,
            discountPercent,
            vatRatePercent,
            netAmount,
            vatAmount,
            grossAmount);

        SalesInvoiceId = salesInvoiceId;
        LineNumber = lineNumber;
        ItemCode = NormalizeOptional(itemCode, 100);
        Description = NormalizeRequired(description, nameof(description), 500);
        Quantity = quantity;
        UnitOfMeasure = NormalizeOptional(unitOfMeasure, 32);
        UnitPrice = unitPrice;
        DiscountPercent = discountPercent;
        VatCodeId = vatCodeId;
        VatCode = NormalizeRequired(vatCode, nameof(vatCode), 32).ToUpperInvariant();
        VatRatePercent = vatRatePercent;
        NetAmount = netAmount;
        VatAmount = vatAmount;
        GrossAmount = grossAmount;
    }

    public Guid SalesInvoiceId { get; private set; }

    public int LineNumber { get; private set; }

    public string? ItemCode { get; private set; }

    public string Description { get; private set; } = null!;

    public decimal Quantity { get; private set; }

    public string? UnitOfMeasure { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal DiscountPercent { get; private set; }

    public Guid VatCodeId { get; private set; }

    public string VatCode { get; private set; } = null!;

    public decimal VatRatePercent { get; private set; }

    public decimal NetAmount { get; private set; }

    public decimal VatAmount { get; private set; }

    public decimal GrossAmount { get; private set; }

    public static SalesInvoiceLine Create(
        Guid organizationId,
        Guid salesInvoiceId,
        int lineNumber,
        string? itemCode,
        string description,
        decimal quantity,
        string? unitOfMeasure,
        decimal unitPrice,
        decimal discountPercent,
        Guid vatCodeId,
        string vatCode,
        decimal vatRatePercent,
        decimal netAmount,
        decimal vatAmount,
        decimal grossAmount,
        DateTimeOffset now)
    {
        return new SalesInvoiceLine(
            Guid.NewGuid(),
            organizationId,
            salesInvoiceId,
            lineNumber,
            itemCode,
            description,
            quantity,
            unitOfMeasure,
            unitPrice,
            discountPercent,
            vatCodeId,
            vatCode,
            vatRatePercent,
            netAmount,
            vatAmount,
            grossAmount,
            now);
    }

    public void Update(
        string? itemCode,
        string description,
        decimal quantity,
        string? unitOfMeasure,
        decimal unitPrice,
        decimal discountPercent,
        Guid vatCodeId,
        string vatCode,
        decimal vatRatePercent,
        decimal netAmount,
        decimal vatAmount,
        decimal grossAmount,
        DateTimeOffset now)
    {
        if (vatCodeId == Guid.Empty)
        {
            throw new ArgumentException(
                "VAT code id is required.",
                nameof(vatCodeId));
        }

        ValidateCommercialValues(
            quantity,
            unitPrice,
            discountPercent,
            vatRatePercent,
            netAmount,
            vatAmount,
            grossAmount);

        ItemCode = NormalizeOptional(itemCode, 100);
        Description = NormalizeRequired(description, nameof(description), 500);
        Quantity = quantity;
        UnitOfMeasure = NormalizeOptional(unitOfMeasure, 32);
        UnitPrice = unitPrice;
        DiscountPercent = discountPercent;
        VatCodeId = vatCodeId;
        VatCode = NormalizeRequired(vatCode, nameof(vatCode), 32).ToUpperInvariant();
        VatRatePercent = vatRatePercent;
        NetAmount = netAmount;
        VatAmount = vatAmount;
        GrossAmount = grossAmount;
        Touch(now);
    }

    private static void ValidateIds(
        Guid salesInvoiceId,
        Guid vatCodeId)
    {
        if (salesInvoiceId == Guid.Empty)
        {
            throw new ArgumentException(
                "Sales invoice id is required.",
                nameof(salesInvoiceId));
        }

        if (vatCodeId == Guid.Empty)
        {
            throw new ArgumentException(
                "VAT code id is required.",
                nameof(vatCodeId));
        }
    }

    private static void ValidateLineNumber(int lineNumber)
    {
        if (lineNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lineNumber),
                "Invoice line number must be positive.");
        }
    }

    private static void ValidateCommercialValues(
        decimal quantity,
        decimal unitPrice,
        decimal discountPercent,
        decimal vatRatePercent,
        decimal netAmount,
        decimal vatAmount,
        decimal grossAmount)
    {
        if (quantity <= 0m || decimal.Round(quantity, 6) != quantity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be positive and support at most six decimal places.");
        }

        if (unitPrice < 0m || decimal.Round(unitPrice, 4) != unitPrice)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unitPrice),
                "Unit price cannot be negative and supports at most four decimal places.");
        }

        if (discountPercent is < 0m or > 100m ||
            decimal.Round(discountPercent, 4) != discountPercent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(discountPercent),
                "Discount percent must be between 0 and 100 and support at most four decimal places.");
        }

        if (vatRatePercent is < 0m or > 100m ||
            decimal.Round(vatRatePercent, 4) != vatRatePercent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vatRatePercent),
                "VAT rate must be between 0 and 100 and support at most four decimal places.");
        }

        if (netAmount < 0m || vatAmount < 0m || grossAmount < 0m ||
            netAmount + vatAmount != grossAmount)
        {
            throw new ArgumentException(
                "Invoice line amounts are invalid.");
        }
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
