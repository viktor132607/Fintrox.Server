using Fintrox.Domain.Common;

namespace Fintrox.Domain.Sales;

public sealed class SalesInvoice : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private SalesInvoice()
    {
    }

    private SalesInvoice(
        Guid id,
        Guid organizationId,
        Guid counterpartyId,
        DateOnly invoiceDate,
        DateOnly dueDate,
        Guid currencyId,
        string customerName,
        string? customerLegalName,
        string? customerRegistrationNumber,
        string? customerVatNumber,
        string customerCountryCode,
        string? customerAddressLine1,
        string? customerAddressLine2,
        string? customerCity,
        string? customerPostalCode,
        string currencyCode,
        string? notes,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        ValidateIds(counterpartyId, currencyId);
        ValidateDates(invoiceDate, dueDate);

        CounterpartyId = counterpartyId;
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        CurrencyId = currencyId;
        CustomerName = NormalizeRequired(customerName, nameof(customerName), 200);
        CustomerLegalName = NormalizeOptional(customerLegalName, 200);
        CustomerRegistrationNumber = NormalizeOptional(customerRegistrationNumber, 64);
        CustomerVatNumber = NormalizeOptional(customerVatNumber, 64);
        CustomerCountryCode = NormalizeCountryCode(customerCountryCode);
        CustomerAddressLine1 = NormalizeOptional(customerAddressLine1, 240);
        CustomerAddressLine2 = NormalizeOptional(customerAddressLine2, 240);
        CustomerCity = NormalizeOptional(customerCity, 120);
        CustomerPostalCode = NormalizeOptional(customerPostalCode, 32);
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        Notes = NormalizeOptional(notes, 1000);
        Status = SalesInvoiceStatus.Draft;
    }

    public string? Number { get; private set; }

    public SalesInvoiceStatus Status { get; private set; }

    public Guid CounterpartyId { get; private set; }

    public DateOnly InvoiceDate { get; private set; }

    public DateOnly DueDate { get; private set; }

    public Guid CurrencyId { get; private set; }

    public Guid? BaseCurrencyId { get; private set; }

    public string CurrencyCode { get; private set; } = null!;

    public string? BaseCurrencyCode { get; private set; }

    public decimal? ExchangeRate { get; private set; }

    public string CustomerName { get; private set; } = null!;

    public string? CustomerLegalName { get; private set; }

    public string? CustomerRegistrationNumber { get; private set; }

    public string? CustomerVatNumber { get; private set; }

    public string CustomerCountryCode { get; private set; } = null!;

    public string? CustomerAddressLine1 { get; private set; }

    public string? CustomerAddressLine2 { get; private set; }

    public string? CustomerCity { get; private set; }

    public string? CustomerPostalCode { get; private set; }

    public decimal NetTotal { get; private set; }

    public decimal VatTotal { get; private set; }

    public decimal GrossTotal { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset? IssuedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public static SalesInvoice CreateDraft(
        Guid organizationId,
        Guid counterpartyId,
        DateOnly invoiceDate,
        DateOnly dueDate,
        Guid currencyId,
        string customerName,
        string? customerLegalName,
        string? customerRegistrationNumber,
        string? customerVatNumber,
        string customerCountryCode,
        string? customerAddressLine1,
        string? customerAddressLine2,
        string? customerCity,
        string? customerPostalCode,
        string currencyCode,
        string? notes,
        DateTimeOffset now)
    {
        return new SalesInvoice(
            Guid.NewGuid(),
            organizationId,
            counterpartyId,
            invoiceDate,
            dueDate,
            currencyId,
            customerName,
            customerLegalName,
            customerRegistrationNumber,
            customerVatNumber,
            customerCountryCode,
            customerAddressLine1,
            customerAddressLine2,
            customerCity,
            customerPostalCode,
            currencyCode,
            notes,
            now);
    }

    public void UpdateDraft(
        Guid counterpartyId,
        DateOnly invoiceDate,
        DateOnly dueDate,
        Guid currencyId,
        string customerName,
        string? customerLegalName,
        string? customerRegistrationNumber,
        string? customerVatNumber,
        string customerCountryCode,
        string? customerAddressLine1,
        string? customerAddressLine2,
        string? customerCity,
        string? customerPostalCode,
        string currencyCode,
        string? notes,
        DateTimeOffset now)
    {
        EnsureDraft();
        ValidateIds(counterpartyId, currencyId);
        ValidateDates(invoiceDate, dueDate);

        CounterpartyId = counterpartyId;
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        CurrencyId = currencyId;
        CustomerName = NormalizeRequired(customerName, nameof(customerName), 200);
        CustomerLegalName = NormalizeOptional(customerLegalName, 200);
        CustomerRegistrationNumber = NormalizeOptional(customerRegistrationNumber, 64);
        CustomerVatNumber = NormalizeOptional(customerVatNumber, 64);
        CustomerCountryCode = NormalizeCountryCode(customerCountryCode);
        CustomerAddressLine1 = NormalizeOptional(customerAddressLine1, 240);
        CustomerAddressLine2 = NormalizeOptional(customerAddressLine2, 240);
        CustomerCity = NormalizeOptional(customerCity, 120);
        CustomerPostalCode = NormalizeOptional(customerPostalCode, 32);
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        Notes = NormalizeOptional(notes, 1000);
        Touch(now);
    }

    public void SetTotals(
        decimal netTotal,
        decimal vatTotal,
        decimal grossTotal,
        DateTimeOffset now)
    {
        EnsureDraft();

        if (netTotal < 0m || vatTotal < 0m || grossTotal < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(netTotal),
                "Invoice totals cannot be negative.");
        }

        if (netTotal + vatTotal != grossTotal)
        {
            throw new ArgumentException(
                "Invoice gross total must equal net total plus VAT total.");
        }

        NetTotal = netTotal;
        VatTotal = vatTotal;
        GrossTotal = grossTotal;
        Touch(now);
    }

    public void Issue(
        string number,
        Guid baseCurrencyId,
        string baseCurrencyCode,
        string currencyCode,
        string customerName,
        string? customerLegalName,
        string? customerRegistrationNumber,
        string? customerVatNumber,
        string customerCountryCode,
        string? customerAddressLine1,
        string? customerAddressLine2,
        string? customerCity,
        string? customerPostalCode,
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

        var normalizedNumber = number?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedNumber) ||
            normalizedNumber.Length > 40)
        {
            throw new ArgumentException(
                "Invoice number is required and cannot exceed 40 characters.",
                nameof(number));
        }

        if (exchangeRate <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(exchangeRate),
                "Invoice exchange rate must be positive.");
        }

        Number = normalizedNumber;
        BaseCurrencyId = baseCurrencyId;
        BaseCurrencyCode = NormalizeCurrencyCode(baseCurrencyCode);
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        CustomerName = NormalizeRequired(customerName, nameof(customerName), 200);
        CustomerLegalName = NormalizeOptional(customerLegalName, 200);
        CustomerRegistrationNumber = NormalizeOptional(customerRegistrationNumber, 64);
        CustomerVatNumber = NormalizeOptional(customerVatNumber, 64);
        CustomerCountryCode = NormalizeCountryCode(customerCountryCode);
        CustomerAddressLine1 = NormalizeOptional(customerAddressLine1, 240);
        CustomerAddressLine2 = NormalizeOptional(customerAddressLine2, 240);
        CustomerCity = NormalizeOptional(customerCity, 120);
        CustomerPostalCode = NormalizeOptional(customerPostalCode, 32);
        ExchangeRate = exchangeRate;
        Status = SalesInvoiceStatus.Issued;
        IssuedAtUtc = now;
        Touch(now);
    }

    public void Cancel(
        string reason,
        DateTimeOffset now)
    {
        if (Status != SalesInvoiceStatus.Issued)
        {
            throw new InvalidOperationException(
                "Only an issued sales invoice can be cancelled.");
        }

        var normalizedReason = NormalizeRequired(
            reason,
            nameof(reason),
            500);

        Status = SalesInvoiceStatus.Cancelled;
        CancellationReason = normalizedReason;
        CancelledAtUtc = now;
        Touch(now);
    }

    public void EnsureDraft()
    {
        if (Status != SalesInvoiceStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft sales invoices can be edited.");
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
        DateOnly invoiceDate,
        DateOnly dueDate)
    {
        if (dueDate < invoiceDate)
        {
            throw new ArgumentException(
                "Due date cannot be before invoice date.",
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
