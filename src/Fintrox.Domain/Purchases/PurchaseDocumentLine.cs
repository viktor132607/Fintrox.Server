using Fintrox.Domain.Common;

namespace Fintrox.Domain.Purchases;

public sealed class PurchaseDocumentLine : OrganizationScopedAuditableEntity
{
    private PurchaseDocumentLine()
    {
    }

    private PurchaseDocumentLine(
        Guid id,
        Guid organizationId,
        Guid purchaseDocumentId,
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
        decimal recoverableVatPercent,
        decimal netAmount,
        decimal vatAmount,
        decimal recoverableVatAmount,
        decimal nonRecoverableVatAmount,
        decimal grossAmount,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        ValidateIds(purchaseDocumentId, vatCodeId);
        ValidateLineNumber(lineNumber);
        ValidateValues(
            quantity,
            unitPrice,
            discountPercent,
            vatRatePercent,
            recoverableVatPercent,
            netAmount,
            vatAmount,
            recoverableVatAmount,
            nonRecoverableVatAmount,
            grossAmount);

        PurchaseDocumentId = purchaseDocumentId;
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
        RecoverableVatPercent = recoverableVatPercent;
        NetAmount = netAmount;
        VatAmount = vatAmount;
        RecoverableVatAmount = recoverableVatAmount;
        NonRecoverableVatAmount = nonRecoverableVatAmount;
        GrossAmount = grossAmount;
    }

    public Guid PurchaseDocumentId { get; private set; }

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

    public decimal RecoverableVatPercent { get; private set; }

    public decimal NetAmount { get; private set; }

    public decimal VatAmount { get; private set; }

    public decimal RecoverableVatAmount { get; private set; }

    public decimal NonRecoverableVatAmount { get; private set; }

    public decimal GrossAmount { get; private set; }

    public static PurchaseDocumentLine Create(
        Guid organizationId,
        Guid purchaseDocumentId,
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
        decimal recoverableVatPercent,
        decimal netAmount,
        decimal vatAmount,
        decimal recoverableVatAmount,
        decimal nonRecoverableVatAmount,
        decimal grossAmount,
        DateTimeOffset now)
    {
        return new PurchaseDocumentLine(
            Guid.NewGuid(),
            organizationId,
            purchaseDocumentId,
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
            recoverableVatPercent,
            netAmount,
            vatAmount,
            recoverableVatAmount,
            nonRecoverableVatAmount,
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
        decimal recoverableVatPercent,
        decimal netAmount,
        decimal vatAmount,
        decimal recoverableVatAmount,
        decimal nonRecoverableVatAmount,
        decimal grossAmount,
        DateTimeOffset now)
    {
        if (vatCodeId == Guid.Empty)
        {
            throw new ArgumentException(
                "VAT code id is required.",
                nameof(vatCodeId));
        }

        ValidateValues(
            quantity,
            unitPrice,
            discountPercent,
            vatRatePercent,
            recoverableVatPercent,
            netAmount,
            vatAmount,
            recoverableVatAmount,
            nonRecoverableVatAmount,
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
        RecoverableVatPercent = recoverableVatPercent;
        NetAmount = netAmount;
        VatAmount = vatAmount;
        RecoverableVatAmount = recoverableVatAmount;
        NonRecoverableVatAmount = nonRecoverableVatAmount;
        GrossAmount = grossAmount;
        Touch(now);
    }

    private static void ValidateIds(
        Guid purchaseDocumentId,
        Guid vatCodeId)
    {
        if (purchaseDocumentId == Guid.Empty ||
            vatCodeId == Guid.Empty)
        {
            throw new ArgumentException(
                "Purchase document and VAT code ids are required.");
        }
    }

    private static void ValidateLineNumber(int lineNumber)
    {
        if (lineNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lineNumber),
                "Purchase line number must be positive.");
        }
    }

    private static void ValidateValues(
        decimal quantity,
        decimal unitPrice,
        decimal discountPercent,
        decimal vatRatePercent,
        decimal recoverableVatPercent,
        decimal netAmount,
        decimal vatAmount,
        decimal recoverableVatAmount,
        decimal nonRecoverableVatAmount,
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
                "Discount percent must be between 0 and 100.");
        }

        if (vatRatePercent is < 0m or > 100m ||
            recoverableVatPercent is < 0m or > 100m ||
            decimal.Round(vatRatePercent, 4) != vatRatePercent ||
            decimal.Round(recoverableVatPercent, 4) != recoverableVatPercent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vatRatePercent),
                "VAT percentages must be between 0 and 100 and support at most four decimal places.");
        }

        if (netAmount < 0m ||
            vatAmount < 0m ||
            recoverableVatAmount < 0m ||
            nonRecoverableVatAmount < 0m ||
            grossAmount < 0m ||
            netAmount + vatAmount != grossAmount ||
            recoverableVatAmount + nonRecoverableVatAmount != vatAmount)
        {
            throw new ArgumentException(
                "Purchase line amounts are inconsistent.");
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
