namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyConversionQuote. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyConversionQuote : OrganizationEntity
{
    public CurrencyConversionQuote(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string sourceCurrency,
        string targetCurrency,
        decimal sourceAmount,
        decimal convertedAmount,
        DateTimeOffset quotedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SourceCurrency = DomainRules.Currency(sourceCurrency, nameof(sourceCurrency));
        TargetCurrency = DomainRules.Currency(targetCurrency, nameof(targetCurrency));
        SourceAmount = DomainRules.Positive(sourceAmount, nameof(sourceAmount));
        ConvertedAmount = DomainRules.Nonnegative(convertedAmount, nameof(convertedAmount));
        QuotedAtUtc = DomainRules.Instant(quotedAtUtc, nameof(quotedAtUtc));
    }

    public string SourceCurrency { get; }
    public string TargetCurrency { get; }
    public decimal SourceAmount { get; }
    public decimal ConvertedAmount { get; }
    public DateTimeOffset QuotedAtUtc { get; }
}
