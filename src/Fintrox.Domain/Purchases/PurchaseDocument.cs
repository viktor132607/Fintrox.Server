using Fintrox.Domain.Common;

namespace Fintrox.Domain.Purchases;

public sealed class PurchaseDocument : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private PurchaseDocument()
    {
    }

    private PurchaseDocument(
        Guid id,
        Guid organizationId,
        PurchaseDocumentType type,
        Guid counterpartyId,
        string? supplierDocumentNumber,
        DateOnly documentDate,
        DateOnly dueDate,
        Guid currencyId,
        string supplierName,
        string? supplierLegalName,
        string? supplierRegistrationNumber,
        string? supplierVatNumber,
        string supplierCountryCode,
        string? supplierAddressLine1,
        string? supplierAddressLine2,
        string? supplierCity,
        string? supplierPostalCode,
        string currencyCode,
        string? notes,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        ValidateIds(counterpartyId, currencyId);
        ValidateDates(documentDate, dueDate);

        Type = type;
        CounterpartyId = counterpartyId;
        SupplierDocumentNumber = NormalizeOptionalIdentifier(
            supplierDocumentNumber,
            80);
        DocumentDate = documentDate;
        DueDate = dueDate;
        CurrencyId = currencyId;
        SupplierName = NormalizeRequired(
            supplierName,
            nameof(supplierName),
            200);
        SupplierLegalName = NormalizeOptional(supplierLegalName, 200);
        SupplierRegistrationNumber = NormalizeOptional(
            supplierRegistrationNumber,
            64);
        SupplierVatNumber = NormalizeOptional(supplierVatNumber, 64);
        SupplierCountryCode = NormalizeCountryCode(supplierCountryCode);
        SupplierAddressLine1 = NormalizeOptional(supplierAddressLine1, 240);
        SupplierAddressLine2 = NormalizeOptional(supplierAddressLine2, 240);
        SupplierCity = NormalizeOptional(supplierCity, 120);
        SupplierPostalCode = NormalizeOptional(supplierPostalCode, 32);
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        Notes = NormalizeOptional(notes, 1000);
        Status = PurchaseDocumentStatus.Draft;
    }

    public string? InternalNumber { get; private set; }

    public PurchaseDocumentType Type { get; private set; }

    public PurchaseDocumentStatus Status { get; private set; }

    public Guid CounterpartyId { get; private set; }

    public string? SupplierDocumentNumber { get; private set; }

    public DateOnly DocumentDate { get; private set; }

    public DateOnly DueDate { get; private set; }

    public Guid CurrencyId { get; private set; }

    public Guid? BaseCurrencyId { get; private set; }

    public string CurrencyCode { get; private set; } = null!;

    public string? BaseCurrencyCode { get; private set; }

    public decimal? ExchangeRate { get; private set; }

    public string SupplierName { get; private set; } = null!;

    public string? SupplierLegalName { get; private set; }

    public string? SupplierRegistrationNumber { get; private set; }

    public string? SupplierVatNumber { get; private set; }

    public string SupplierCountryCode { get; private set; } = null!;

    public string? SupplierAddressLine1 { get; private set; }

    public string? SupplierAddressLine2 { get; private set; }

    public string? SupplierCity { get; private set; }

    public string? SupplierPostalCode { get; private set; }

    public decimal NetTotal { get; private set; }

    public decimal VatTotal { get; private set; }

    public decimal RecoverableVatTotal { get; private set; }

    public decimal NonRecoverableVatTotal { get; private set; }

    public decimal GrossTotal { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset? ReceivedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public static PurchaseDocument CreateDraft(
        Guid organizationId,
        PurchaseDocumentType type,
        Guid counterpartyId,
        string? supplierDocumentNumber,
        DateOnly documentDate,
        DateOnly dueDate,
        Guid currencyId,
        string supplierName,
        string? supplierLegalName,
        string? supplierRegistrationNumber,
        string? supplierVatNumber,
        string supplierCountryCode,
        string? supplierAddressLine1,
        string? supplierAddressLine2,
        string? supplierCity,
        string? supplierPostalCode,
        string currencyCode,
        string? notes,
        DateTimeOffset now)
    {
        return new PurchaseDocument(
            Guid.NewGuid(),
            organizationId,
            type,
            counterpartyId,
            supplierDocumentNumber,
            documentDate,
            dueDate,
            currencyId,
            supplierName,
            supplierLegalName,
            supplierRegistrationNumber,
            supplierVatNumber,
            supplierCountryCode,
            supplierAddressLine1,
            supplierAddressLine2,
            supplierCity,
            supplierPostalCode,
            currencyCode,
            notes,
            now);
    }

    public void UpdateDraft(
        PurchaseDocumentType type,
        Guid counterpartyId,
        string? supplierDocumentNumber,
        DateOnly documentDate,
        DateOnly dueDate,
        Guid currencyId,
        string supplierName,
        string? supplierLegalName,
        string? supplierRegistrationNumber,
        string? supplierVatNumber,
        string supplierCountryCode,
        string? supplierAddressLine1,
        string? supplierAddressLine2,
        string? supplierCity,
        string? supplierPostalCode,
        string currencyCode,
        string? notes,
        DateTimeOffset now)
    {
        EnsureDraft();
        ValidateIds(counterpartyId, currencyId);
        ValidateDates(documentDate, dueDate);

        Type = type;
        CounterpartyId = counterpartyId;
        SupplierDocumentNumber = NormalizeOptionalIdentifier(
            supplierDocumentNumber,
            80);
        DocumentDate = documentDate;
        DueDate = dueDate;
        CurrencyId = currencyId;
        SupplierName = NormalizeRequired(
            supplierName,
            nameof(supplierName),
            200);
        SupplierLegalName = NormalizeOptional(supplierLegalName, 200);
        SupplierRegistrationNumber = NormalizeOptional(
            supplierRegistrationNumber,
            64);
        SupplierVatNumber = NormalizeOptional(supplierVatNumber, 64);
        SupplierCountryCode = NormalizeCountryCode(supplierCountryCode);
        SupplierAddressLine1 = NormalizeOptional(supplierAddressLine1, 240);
        SupplierAddressLine2 = NormalizeOptional(supplierAddressLine2, 240);
        SupplierCity = NormalizeOptional(supplierCity, 120);
        SupplierPostalCode = NormalizeOptional(supplierPostalCode, 32);
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        Notes = NormalizeOptional(notes, 1000);
        Touch(now);
    }

    public void SetTotals(
        decimal netTotal,
        decimal vatTotal,
        decimal recoverableVatTotal,
        decimal nonRecoverableVatTotal,
        decimal grossTotal,
        DateTimeOffset now)
    {
        EnsureDraft();

        if (netTotal < 0m ||
            vatTotal < 0m ||
            recoverableVatTotal < 0m ||
            nonRecoverableVatTotal < 0m ||
            grossTotal < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(netTotal),
                "Purchase document totals cannot be negative.");
        }

        if (netTotal + vatTotal != grossTotal ||
            recoverableVatTotal + nonRecoverableVatTotal != vatTotal)
        {
            throw new ArgumentException(
                "Purchase document totals are inconsistent.");
        }

        NetTotal = netTotal;
        VatTotal = vatTotal;
        RecoverableVatTotal = recoverableVatTotal;
        NonRecoverableVatTotal = nonRecoverableVatTotal;
        GrossTotal = grossTotal;
        Touch(now);
    }

    public void Receive(
        string internalNumber,
        string? supplierDocumentNumber,
        Guid baseCurrencyId,
        string baseCurrencyCode,
        string currencyCode,
        string supplierName,
        string? supplierLegalName,
        string? supplierRegistrationNumber,
        string? supplierVatNumber,
        string supplierCountryCode,
        string? supplierAddressLine1,
        string? supplierAddressLine2,
        string? supplierCity,
        string? supplierPostalCode,
        decimal exchangeRate,
        DateTimeOffset now)
    {
        EnsureDraft();

        if (baseCurrencyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Base currency id is required.",
                nameof(baseCurrencyId));
        }

        var normalizedInternalNumber = internalNumber?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedInternalNumber) ||
            normalizedInternalNumber.Length > 40)
        {
            throw new ArgumentException(
                "Internal purchase number is required and cannot exceed 40 characters.",
                nameof(internalNumber));
        }

        var normalizedSupplierNumber = NormalizeOptionalIdentifier(
            supplierDocumentNumber,
            80);

        if (Type == PurchaseDocumentType.Invoice &&
            string.IsNullOrWhiteSpace(normalizedSupplierNumber))
        {
            throw new ArgumentException(
                "Supplier document number is required for purchase invoices.",
                nameof(supplierDocumentNumber));
        }

        if (exchangeRate <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(exchangeRate),
                "Purchase document exchange rate must be positive.");
        }

        InternalNumber = normalizedInternalNumber;
        SupplierDocumentNumber = normalizedSupplierNumber;
        BaseCurrencyId = baseCurrencyId;
        BaseCurrencyCode = NormalizeCurrencyCode(baseCurrencyCode);
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        SupplierName = NormalizeRequired(
            supplierName,
            nameof(supplierName),
            200);
        SupplierLegalName = NormalizeOptional(supplierLegalName, 200);
        SupplierRegistrationNumber = NormalizeOptional(
            supplierRegistrationNumber,
            64);
        SupplierVatNumber = NormalizeOptional(supplierVatNumber, 64);
        SupplierCountryCode = NormalizeCountryCode(supplierCountryCode);
        SupplierAddressLine1 = NormalizeOptional(supplierAddressLine1, 240);
        SupplierAddressLine2 = NormalizeOptional(supplierAddressLine2, 240);
        SupplierCity = NormalizeOptional(supplierCity, 120);
        SupplierPostalCode = NormalizeOptional(supplierPostalCode, 32);
        ExchangeRate = exchangeRate;
        Status = PurchaseDocumentStatus.Received;
        ReceivedAtUtc = now;
        Touch(now);
    }

    public void Cancel(
        string reason,
        DateTimeOffset now)
    {
        if (Status != PurchaseDocumentStatus.Received)
        {
            throw new InvalidOperationException(
                "Only a received purchase document can be cancelled.");
        }

        Status = PurchaseDocumentStatus.Cancelled;
        CancellationReason = NormalizeRequired(
            reason,
            nameof(reason),
            500);
        CancelledAtUtc = now;
        Touch(now);
    }

    public void EnsureDraft()
    {
        if (Status != PurchaseDocumentStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft purchase documents can be edited.");
        }
    }

    private static void ValidateIds(
        Guid counterpartyId,
        Guid currencyId)
    {
        if (counterpartyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Counterparty id is required.",
                nameof(counterpartyId));
        }

        if (currencyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Currency id is required.",
                nameof(currencyId));
        }
    }

    private static void ValidateDates(
        DateOnly documentDate,
        DateOnly dueDate)
    {
        if (dueDate < documentDate)
        {
            throw new ArgumentException(
                "Due date cannot be before document date.",
                nameof(dueDate));
        }
    }

    private static string NormalizeCountryCode(string value)
    {
        var normalized = NormalizeRequired(value, nameof(value), 2)
            .ToUpperInvariant();

        if (normalized.Length != 2 ||
            !normalized.All(char.IsAsciiLetter))
        {
            throw new ArgumentException(
                "Country code must contain exactly two ASCII letters.",
                nameof(value));
        }

        return normalized;
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

    private static string? NormalizeOptionalIdentifier(
        string? value,
        int maxLength)
    {
        var normalized = NormalizeOptional(value, maxLength);
        return normalized?.ToUpperInvariant();
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
