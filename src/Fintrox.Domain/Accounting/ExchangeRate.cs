using Fintrox.Domain.Common;

namespace Fintrox.Domain.Accounting;

public sealed class ExchangeRate : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private ExchangeRate()
    {
    }

    private ExchangeRate(
        Guid id,
        Guid organizationId,
        Guid baseCurrencyId,
        Guid quoteCurrencyId,
        DateOnly effectiveDate,
        decimal rate,
        string? source,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        ValidateCurrencies(baseCurrencyId, quoteCurrencyId);
        ValidateRate(rate);

        BaseCurrencyId = baseCurrencyId;
        QuoteCurrencyId = quoteCurrencyId;
        EffectiveDate = effectiveDate;
        Rate = rate;
        Source = NormalizeSource(source);
    }

    public Guid BaseCurrencyId { get; private set; }

    public Guid QuoteCurrencyId { get; private set; }

    public DateOnly EffectiveDate { get; private set; }

    public decimal Rate { get; private set; }

    public string? Source { get; private set; }

    public static ExchangeRate Create(
        Guid organizationId,
        Guid baseCurrencyId,
        Guid quoteCurrencyId,
        DateOnly effectiveDate,
        decimal rate,
        string? source,
        DateTimeOffset now)
    {
        return new ExchangeRate(
            Guid.NewGuid(),
            organizationId,
            baseCurrencyId,
            quoteCurrencyId,
            effectiveDate,
            rate,
            source,
            now);
    }

    public void Update(
        decimal rate,
        string? source,
        DateTimeOffset now)
    {
        ValidateRate(rate);
        Rate = rate;
        Source = NormalizeSource(source);
        Touch(now);
    }

    private static void ValidateCurrencies(
        Guid baseCurrencyId,
        Guid quoteCurrencyId)
    {
        if (baseCurrencyId == Guid.Empty ||
            quoteCurrencyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Both exchange-rate currencies are required.");
        }

        if (baseCurrencyId == quoteCurrencyId)
        {
            throw new ArgumentException(
                "Base and quote currencies must be different.");
        }
    }

    private static void ValidateRate(decimal rate)
    {
        if (rate <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rate),
                "Exchange rate must be positive.");
        }

        if (decimal.Round(rate, 10) != rate)
        {
            throw new ArgumentException(
                "Exchange rate supports at most ten decimal places.",
                nameof(rate));
        }
    }

    private static string? NormalizeSource(string? value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > 120)
        {
            throw new ArgumentException(
                "Exchange-rate source cannot exceed 120 characters.",
                nameof(value));
        }

        return normalized;
    }
}
