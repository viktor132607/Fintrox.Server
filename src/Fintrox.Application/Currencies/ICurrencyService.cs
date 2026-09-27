using Fintrox.Contracts.Currencies;

namespace Fintrox.Application.Currencies;

public interface ICurrencyService
{
    Task<IReadOnlyList<CurrencyResponse>> ListCurrenciesAsync(
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<CurrencyResponse?> GetCurrencyAsync(
        Guid currencyId,
        CancellationToken cancellationToken);

    Task<CurrencyResponse> CreateCurrencyAsync(
        CreateCurrencyRequest request,
        CancellationToken cancellationToken);

    Task<CurrencyResponse?> UpdateCurrencyAsync(
        Guid currencyId,
        UpdateCurrencyRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateCurrencyAsync(
        Guid currencyId,
        CancellationToken cancellationToken);

    Task<CurrencyResponse?> ActivateCurrencyAsync(
        Guid currencyId,
        CancellationToken cancellationToken);

    Task<CurrencyResponse?> SetBaseCurrencyAsync(
        Guid currencyId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ExchangeRateResponse>> ListExchangeRatesAsync(
        Guid? baseCurrencyId,
        Guid? quoteCurrencyId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken);

    Task<ExchangeRateResponse?> GetExchangeRateAsync(
        Guid exchangeRateId,
        CancellationToken cancellationToken);

    Task<ExchangeRateResponse?> GetLatestExchangeRateAsync(
        Guid baseCurrencyId,
        Guid quoteCurrencyId,
        DateOnly? asOfDate,
        CancellationToken cancellationToken);

    Task<ExchangeRateResponse> CreateExchangeRateAsync(
        CreateExchangeRateRequest request,
        CancellationToken cancellationToken);

    Task<ExchangeRateResponse?> UpdateExchangeRateAsync(
        Guid exchangeRateId,
        UpdateExchangeRateRequest request,
        CancellationToken cancellationToken);
}
