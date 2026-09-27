using Fintrox.Domain.Common;

namespace Fintrox.Domain.Payments;

public sealed class Payment : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private Payment()
    {
    }

    private Payment(
        Guid id,
        Guid organizationId,
        PaymentDirection direction,
        Guid counterpartyId,
        DateOnly paymentDate,
        PaymentMethod method,
        Guid currencyId,
        string currencyCode,
        decimal amount,
        string counterpartyName,
        string? counterpartyRegistrationNumber,
        string? counterpartyVatNumber,
        string? reference,
        string? notes,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        ValidateCore(counterpartyId, currencyId, amount);

        Direction = direction;
        CounterpartyId = counterpartyId;
        PaymentDate = paymentDate;
        Method = method;
        CurrencyId = currencyId;
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        Amount = amount;
        CounterpartyName = NormalizeRequired(
            counterpartyName,
            nameof(counterpartyName),
            200);
        CounterpartyRegistrationNumber = NormalizeOptional(
            counterpartyRegistrationNumber,
            64);
        CounterpartyVatNumber = NormalizeOptional(
            counterpartyVatNumber,
            64);
        Reference = NormalizeOptional(reference, 120);
        Notes = NormalizeOptional(notes, 1000);
        Status = PaymentStatus.Draft;
    }

    public string? InternalNumber { get; private set; }

    public PaymentDirection Direction { get; private set; }

    public PaymentStatus Status { get; private set; }

    public Guid CounterpartyId { get; private set; }

    public string CounterpartyName { get; private set; } = null!;

    public string? CounterpartyRegistrationNumber { get; private set; }

    public string? CounterpartyVatNumber { get; private set; }

    public DateOnly PaymentDate { get; private set; }

    public PaymentMethod Method { get; private set; }

    public Guid CurrencyId { get; private set; }

    public Guid? BaseCurrencyId { get; private set; }

    public string CurrencyCode { get; private set; } = null!;

    public string? BaseCurrencyCode { get; private set; }

    public decimal? ExchangeRate { get; private set; }

    public decimal Amount { get; private set; }

    public decimal AllocatedAmount { get; private set; }

    public string? Reference { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset? ConfirmedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public decimal UnallocatedAmount => Amount - AllocatedAmount;

    public static Payment CreateDraft(
        Guid organizationId,
        PaymentDirection direction,
        Guid counterpartyId,
        DateOnly paymentDate,
        PaymentMethod method,
        Guid currencyId,
        string currencyCode,
        decimal amount,
        string counterpartyName,
        string? counterpartyRegistrationNumber,
        string? counterpartyVatNumber,
        string? reference,
        string? notes,
        DateTimeOffset now)
    {
        return new Payment(
            Guid.NewGuid(),
            organizationId,
            direction,
            counterpartyId,
            paymentDate,
            method,
            currencyId,
            currencyCode,
            amount,
            counterpartyName,
            counterpartyRegistrationNumber,
            counterpartyVatNumber,
            reference,
            notes,
            now);
    }

    public void UpdateDraft(
        PaymentDirection direction,
        Guid counterpartyId,
        DateOnly paymentDate,
        PaymentMethod method,
        Guid currencyId,
        string currencyCode,
        decimal amount,
        string counterpartyName,
        string? counterpartyRegistrationNumber,
        string? counterpartyVatNumber,
        string? reference,
        string? notes,
        DateTimeOffset now)
    {
        EnsureDraft();
        ValidateCore(counterpartyId, currencyId, amount);

        if (amount < AllocatedAmount)
        {
            throw new InvalidOperationException(
                "Payment amount cannot be lower than the allocated amount.");
        }

        var normalizedCurrencyCode = NormalizeCurrencyCode(currencyCode);
        var normalizedCounterpartyName = NormalizeRequired(
            counterpartyName,
            nameof(counterpartyName),
            200);
        var normalizedRegistrationNumber = NormalizeOptional(
            counterpartyRegistrationNumber,
            64);
        var normalizedVatNumber = NormalizeOptional(
            counterpartyVatNumber,
            64);
        var normalizedReference = NormalizeOptional(reference, 120);
        var normalizedNotes = NormalizeOptional(notes, 1000);

        Direction = direction;
        CounterpartyId = counterpartyId;
        PaymentDate = paymentDate;
        Method = method;
        CurrencyId = currencyId;
        CurrencyCode = normalizedCurrencyCode;
        Amount = amount;
        CounterpartyName = normalizedCounterpartyName;
        CounterpartyRegistrationNumber = normalizedRegistrationNumber;
        CounterpartyVatNumber = normalizedVatNumber;
        Reference = normalizedReference;
        Notes = normalizedNotes;
        Touch(now);
    }

    public void SetAllocatedAmount(
        decimal allocatedAmount,
        DateTimeOffset now)
    {
        EnsureDraft();

        if (allocatedAmount < 0m || allocatedAmount > Amount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(allocatedAmount),
                "Allocated amount must be between zero and the payment amount.");
        }

        AllocatedAmount = allocatedAmount;
        Touch(now);
    }

    public void Confirm(
        string internalNumber,
        Guid baseCurrencyId,
        string baseCurrencyCode,
        string currencyCode,
        string counterpartyName,
        string? counterpartyRegistrationNumber,
        string? counterpartyVatNumber,
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

        var normalizedNumber = internalNumber?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedNumber) ||
            normalizedNumber.Length > 40)
        {
            throw new ArgumentException(
                "Payment internal number is required and cannot exceed 40 characters.",
                nameof(internalNumber));
        }

        if (exchangeRate <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(exchangeRate),
                "Payment exchange rate must be positive.");
        }

        if (AllocatedAmount > Amount)
        {
            throw new InvalidOperationException(
                "Allocated amount cannot exceed payment amount.");
        }

        var normalizedBaseCurrencyCode = NormalizeCurrencyCode(baseCurrencyCode);
        var normalizedCurrencyCode = NormalizeCurrencyCode(currencyCode);
        var normalizedCounterpartyName = NormalizeRequired(
            counterpartyName,
            nameof(counterpartyName),
            200);
        var normalizedRegistrationNumber = NormalizeOptional(
            counterpartyRegistrationNumber,
            64);
        var normalizedVatNumber = NormalizeOptional(
            counterpartyVatNumber,
            64);

        InternalNumber = normalizedNumber;
        BaseCurrencyId = baseCurrencyId;
        BaseCurrencyCode = normalizedBaseCurrencyCode;
        CurrencyCode = normalizedCurrencyCode;
        CounterpartyName = normalizedCounterpartyName;
        CounterpartyRegistrationNumber = normalizedRegistrationNumber;
        CounterpartyVatNumber = normalizedVatNumber;
        ExchangeRate = exchangeRate;
        Status = PaymentStatus.Confirmed;
        ConfirmedAtUtc = now;
        Touch(now);
    }

    public void Cancel(
        string reason,
        DateTimeOffset now)
    {
        if (Status != PaymentStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Only a confirmed payment can be cancelled.");
        }

        var normalizedReason = NormalizeRequired(
            reason,
            nameof(reason),
            500);

        Status = PaymentStatus.Cancelled;
        CancellationReason = normalizedReason;
        CancelledAtUtc = now;
        Touch(now);
    }

    public void EnsureDraft()
    {
        if (Status != PaymentStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft payments can be edited.");
        }
    }

    private static void ValidateCore(
        Guid counterpartyId,
        Guid currencyId,
        decimal amount)
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

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Payment amount must be positive.");
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
