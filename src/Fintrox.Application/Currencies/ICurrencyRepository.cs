using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Currencies;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> ListCurrenciesAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<Currency?> GetCurrencyAsync(
        Guid organizationId,
        Guid currencyId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<Currency?> GetBaseCurrencyAsync(
        Guid organizationId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> CurrencyCodeExistsAsync(
        Guid organizationId,
        string code,
        Guid? excludingCurrencyId,
        CancellationToken cancellationToken);

    Task AddCurrencyAsync(
        Currency currency,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ExchangeRate>> ListExchangeRatesAsync(
        Guid organizationId,
        Guid? baseCurrencyId,
        Guid? quoteCurrencyId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken);

    Task<ExchangeRate?> GetExchangeRateAsync(
        Guid organizationId,
        Guid exchangeRateId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<ExchangeRate?> GetLatestExchangeRateAsync(
        Guid organizationId,
        Guid baseCurrencyId,
        Guid quoteCurrencyId,
        DateOnly asOfDate,
        CancellationToken cancellationToken);

    Task<bool> ExchangeRateExistsAsync(
        Guid organizationId,
        Guid baseCurrencyId,
        Guid quoteCurrencyId,
        DateOnly effectiveDate,
        Guid? excludingExchangeRateId,
        CancellationToken cancellationToken);

    Task AddExchangeRateAsync(
        ExchangeRate exchangeRate,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
