using Fintrox.Domain.Common;

namespace Fintrox.Domain.Payments;

public sealed class PaymentAllocation : OrganizationScopedAuditableEntity
{
    private PaymentAllocation()
    {
    }

    private PaymentAllocation(
        Guid id,
        Guid organizationId,
        Guid paymentId,
        int lineNumber,
        PaymentAllocationTargetType targetType,
        Guid? salesInvoiceId,
        Guid? purchaseDocumentId,
        string documentNumber,
        Guid documentCurrencyId,
        string documentCurrencyCode,
        decimal documentExchangeRate,
        decimal documentAmount,
        decimal paymentAmount,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        Validate(
            paymentId,
            lineNumber,
            targetType,
            salesInvoiceId,
            purchaseDocumentId,
            documentCurrencyId,
            documentExchangeRate,
            documentAmount,
            paymentAmount);

        PaymentId = paymentId;
        LineNumber = lineNumber;
        TargetType = targetType;
        SalesInvoiceId = salesInvoiceId;
        PurchaseDocumentId = purchaseDocumentId;
        DocumentNumber = NormalizeRequired(
            documentNumber,
            nameof(documentNumber),
            80);
        DocumentCurrencyId = documentCurrencyId;
        DocumentCurrencyCode = NormalizeCurrencyCode(documentCurrencyCode);
        DocumentExchangeRate = documentExchangeRate;
        DocumentAmount = documentAmount;
        PaymentAmount = paymentAmount;
    }

    public Guid PaymentId { get; private set; }

    public int LineNumber { get; private set; }

    public PaymentAllocationTargetType TargetType { get; private set; }

    public Guid? SalesInvoiceId { get; private set; }

    public Guid? PurchaseDocumentId { get; private set; }

    public string DocumentNumber { get; private set; } = null!;

    public Guid DocumentCurrencyId { get; private set; }

    public string DocumentCurrencyCode { get; private set; } = null!;

    public decimal DocumentExchangeRate { get; private set; }

    public decimal DocumentAmount { get; private set; }

    public decimal PaymentAmount { get; private set; }

    public static PaymentAllocation Create(
        Guid organizationId,
        Guid paymentId,
        int lineNumber,
        PaymentAllocationTargetType targetType,
        Guid? salesInvoiceId,
        Guid? purchaseDocumentId,
        string documentNumber,
        Guid documentCurrencyId,
        string documentCurrencyCode,
        decimal documentExchangeRate,
        decimal documentAmount,
        decimal paymentAmount,
        DateTimeOffset now)
    {
        return new PaymentAllocation(
            Guid.NewGuid(),
            organizationId,
            paymentId,
            lineNumber,
            targetType,
            salesInvoiceId,
            purchaseDocumentId,
            documentNumber,
            documentCurrencyId,
            documentCurrencyCode,
            documentExchangeRate,
            documentAmount,
            paymentAmount,
            now);
    }

    public void Update(
        PaymentAllocationTargetType targetType,
        Guid? salesInvoiceId,
        Guid? purchaseDocumentId,
        string documentNumber,
        Guid documentCurrencyId,
        string documentCurrencyCode,
        decimal documentExchangeRate,
        decimal documentAmount,
        decimal paymentAmount,
        DateTimeOffset now)
    {
        Validate(
            PaymentId,
            LineNumber,
            targetType,
            salesInvoiceId,
            purchaseDocumentId,
            documentCurrencyId,
            documentExchangeRate,
            documentAmount,
            paymentAmount);

        var normalizedDocumentNumber = NormalizeRequired(
            documentNumber,
            nameof(documentNumber),
            80);
        var normalizedCurrencyCode = NormalizeCurrencyCode(documentCurrencyCode);

        TargetType = targetType;
        SalesInvoiceId = salesInvoiceId;
        PurchaseDocumentId = purchaseDocumentId;
        DocumentNumber = normalizedDocumentNumber;
        DocumentCurrencyId = documentCurrencyId;
        DocumentCurrencyCode = normalizedCurrencyCode;
        DocumentExchangeRate = documentExchangeRate;
        DocumentAmount = documentAmount;
        PaymentAmount = paymentAmount;
        Touch(now);
    }

    private static void Validate(
        Guid paymentId,
        int lineNumber,
        PaymentAllocationTargetType targetType,
        Guid? salesInvoiceId,
        Guid? purchaseDocumentId,
        Guid documentCurrencyId,
        decimal documentExchangeRate,
        decimal documentAmount,
        decimal paymentAmount)
    {
        if (paymentId == Guid.Empty ||
            documentCurrencyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Payment and document currency ids are required.");
        }

        if (lineNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lineNumber),
                "Allocation line number must be positive.");
        }

        var validTarget = targetType switch
        {
            PaymentAllocationTargetType.SalesInvoice =>
                salesInvoiceId.HasValue &&
                salesInvoiceId.Value != Guid.Empty &&
                !purchaseDocumentId.HasValue,
            PaymentAllocationTargetType.PurchaseDocument =>
                purchaseDocumentId.HasValue &&
                purchaseDocumentId.Value != Guid.Empty &&
                !salesInvoiceId.HasValue,
            _ => false
        };

        if (!validTarget)
        {
            throw new ArgumentException(
                "Allocation target does not match target type.");
        }

        if (documentExchangeRate <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(documentExchangeRate),
                "Document exchange rate must be positive.");
        }

        if (documentAmount <= 0m || paymentAmount <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(documentAmount),
                "Allocation amounts must be positive.");
        }
    }

    private static string NormalizeCurrencyCode(string value)
    {
        var normalized = NormalizeRequired(value, nameof(value), 3)
            .ToUpperInvariant();

        if (normalized.Length != 3 ||
            !normalized.All(char.IsAsciiLetter))
        {
            throw new ArgumentException(
                "Currency code must contain exactly three ASCII letters.",
                nameof(value));
        }

        return normalized;
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
}
