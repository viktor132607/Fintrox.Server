using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Currencies;
using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Currencies;

public sealed class CurrencyService(
    ICurrencyRepository repository,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : ICurrencyService
{
    public async Task<IReadOnlyList<CurrencyResponse>> ListCurrenciesAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var currencies = await repository.ListCurrenciesAsync(
            organizationId,
            includeInactive,
            cancellationToken);

        return currencies
            .OrderByDescending(c => c.IsBaseCurrency)
            .ThenBy(c => c.Code, StringComparer.Ordinal)
            .Select(MapCurrency)
            .ToArray();
    }

    public async Task<CurrencyResponse?> GetCurrencyAsync(
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var currency = await repository.GetCurrencyAsync(
            organizationId,
            currencyId,
            trackChanges: false,
            cancellationToken);

        return currency is null ? null : MapCurrency(currency);
    }

    public async Task<CurrencyResponse> CreateCurrencyAsync(
        CreateCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var code = NormalizeCurrencyCode(request.Code);

        if (await repository.CurrencyCodeExistsAsync(
                organizationId,
                code,
                excludingCurrencyId: null,
                cancellationToken))
        {
            throw new CurrencyConflictException(
                $"Currency '{code}' already exists in this organization.");
        }

        if (request.IsBaseCurrency &&
            await repository.GetBaseCurrencyAsync(
                organizationId,
                trackChanges: false,
                cancellationToken) is not null)
        {
            throw new CurrencyConflictException(
                "The organization already has a base currency. Use the set-base operation to change it.");
        }

        var currency = Currency.Create(
            organizationId,
            code,
            request.Name,
            request.Symbol,
            request.DecimalPlaces,
            request.IsBaseCurrency,
            timeProvider.GetUtcNow());

        await repository.AddCurrencyAsync(currency, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return MapCurrency(currency);
    }

    public async Task<CurrencyResponse?> UpdateCurrencyAsync(
        Guid currencyId,
        UpdateCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var currency = await repository.GetCurrencyAsync(
            organizationId,
            currencyId,
            trackChanges: true,
            cancellationToken);

        if (currency is null)
        {
            return null;
        }

        var code = NormalizeCurrencyCode(request.Code);

        if (await repository.CurrencyCodeExistsAsync(
                organizationId,
                code,
                currencyId,
                cancellationToken))
        {
            throw new CurrencyConflictException(
                $"Currency '{code}' already exists in this organization.");
        }

        currency.Update(
            code,
            request.Name,
            request.Symbol,
            request.DecimalPlaces,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return MapCurrency(currency);
    }

    public async Task<bool> DeactivateCurrencyAsync(
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var currency = await repository.GetCurrencyAsync(
            organizationId,
            currencyId,
            trackChanges: true,
            cancellationToken);

        if (currency is null)
        {
            return false;
        }

        if (currency.IsBaseCurrency)
        {
            throw new CurrencyConflictException(
                "The base currency cannot be deactivated.");
        }

        currency.Deactivate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<CurrencyResponse?> ActivateCurrencyAsync(
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var currency = await repository.GetCurrencyAsync(
            organizationId,
            currencyId,
            trackChanges: true,
            cancellationToken);

        if (currency is null)
        {
            return null;
        }

        currency.Activate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return MapCurrency(currency);
    }

    public async Task<CurrencyResponse?> SetBaseCurrencyAsync(
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var target = await repository.GetCurrencyAsync(
            organizationId,
            currencyId,
            trackChanges: true,
            cancellationToken);

        if (target is null)
        {
            return null;
        }

        if (!target.IsActive)
        {
            throw new CurrencyConflictException(
                "An inactive currency cannot be set as the base currency.");
        }

        var now = timeProvider.GetUtcNow();
        var current = await repository.GetBaseCurrencyAsync(
            organizationId,
            trackChanges: true,
            cancellationToken);

        if (current is not null && current.Id != target.Id)
        {
            current.ClearBase(now);
        }

        target.SetAsBase(now);
        await repository.SaveChangesAsync(cancellationToken);

        return MapCurrency(target);
    }

    public async Task<IReadOnlyList<ExchangeRateResponse>> ListExchangeRatesAsync(
        Guid? baseCurrencyId,
        Guid? quoteCurrencyId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        ValidateRange(fromDate, toDate);

        var organizationId = currentOrganization.RequireOrganizationId();
        var rates = await repository.ListExchangeRatesAsync(
            organizationId,
            baseCurrencyId,
            quoteCurrencyId,
            fromDate,
            toDate,
            cancellationToken);

        return await MapRatesAsync(
            organizationId,
            rates,
            cancellationToken);
    }

    public async Task<ExchangeRateResponse?> GetExchangeRateAsync(
        Guid exchangeRateId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var rate = await repository.GetExchangeRateAsync(
            organizationId,
            exchangeRateId,
            trackChanges: false,
            cancellationToken);

        if (rate is null)
        {
            return null;
        }

        return (await MapRatesAsync(
            organizationId,
            [rate],
            cancellationToken))[0];
    }

    public async Task<ExchangeRateResponse?> GetLatestExchangeRateAsync(
        Guid baseCurrencyId,
        Guid quoteCurrencyId,
        DateOnly? asOfDate,
        CancellationToken cancellationToken)
    {
        ValidateCurrencyPair(baseCurrencyId, quoteCurrencyId);

        var organizationId = currentOrganization.RequireOrganizationId();
        var effectiveDate = asOfDate ??
            DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var rate = await repository.GetLatestExchangeRateAsync(
            organizationId,
            baseCurrencyId,
            quoteCurrencyId,
            effectiveDate,
            cancellationToken);

        if (rate is null)
        {
            return null;
        }

        return (await MapRatesAsync(
            organizationId,
            [rate],
            cancellationToken))[0];
    }

    public async Task<ExchangeRateResponse> CreateExchangeRateAsync(
        CreateExchangeRateRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCurrencyPair(
            request.BaseCurrencyId,
            request.QuoteCurrencyId);

        var organizationId = currentOrganization.RequireOrganizationId();
        var (baseCurrency, quoteCurrency) = await RequireActiveCurrenciesAsync(
            organizationId,
            request.BaseCurrencyId,
            request.QuoteCurrencyId,
            cancellationToken);

        if (await repository.ExchangeRateExistsAsync(
                organizationId,
                request.BaseCurrencyId,
                request.QuoteCurrencyId,
                request.EffectiveDate,
                excludingExchangeRateId: null,
                cancellationToken))
        {
            throw new CurrencyConflictException(
                "An exchange rate for this currency pair and effective date already exists.");
        }

        var rate = ExchangeRate.Create(
            organizationId,
            request.BaseCurrencyId,
            request.QuoteCurrencyId,
            request.EffectiveDate,
            request.Rate,
            request.Source,
            timeProvider.GetUtcNow());

        await repository.AddExchangeRateAsync(rate, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return MapRate(rate, baseCurrency, quoteCurrency);
    }

    public async Task<ExchangeRateResponse?> UpdateExchangeRateAsync(
        Guid exchangeRateId,
        UpdateExchangeRateRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var rate = await repository.GetExchangeRateAsync(
            organizationId,
            exchangeRateId,
            trackChanges: true,
            cancellationToken);

        if (rate is null)
        {
            return null;
        }

        rate.Update(
            request.Rate,
            request.Source,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);

        var baseCurrency = await repository.GetCurrencyAsync(
            organizationId,
            rate.BaseCurrencyId,
            trackChanges: false,
            cancellationToken);

        var quoteCurrency = await repository.GetCurrencyAsync(
            organizationId,
            rate.QuoteCurrencyId,
            trackChanges: false,
            cancellationToken);

        if (baseCurrency is null || quoteCurrency is null)
        {
            throw new InvalidOperationException(
                "Exchange rate references missing currencies.");
        }

        return MapRate(rate, baseCurrency, quoteCurrency);
    }

    private async Task<(Currency Base, Currency Quote)> RequireActiveCurrenciesAsync(
        Guid organizationId,
        Guid baseCurrencyId,
        Guid quoteCurrencyId,
        CancellationToken cancellationToken)
    {
        var baseCurrency = await repository.GetCurrencyAsync(
            organizationId,
            baseCurrencyId,
            trackChanges: false,
            cancellationToken);

        var quoteCurrency = await repository.GetCurrencyAsync(
            organizationId,
            quoteCurrencyId,
            trackChanges: false,
            cancellationToken);

        if (baseCurrency is null || quoteCurrency is null)
        {
            throw new CurrencyConflictException(
                "Both currencies must exist in this organization.");
        }

        if (!baseCurrency.IsActive || !quoteCurrency.IsActive)
        {
            throw new CurrencyConflictException(
                "Exchange rates can only be created for active currencies.");
        }

        return (baseCurrency, quoteCurrency);
    }

    private async Task<IReadOnlyList<ExchangeRateResponse>> MapRatesAsync(
        Guid organizationId,
        IReadOnlyList<ExchangeRate> rates,
        CancellationToken cancellationToken)
    {
        var currencies = await repository.ListCurrenciesAsync(
            organizationId,
            includeInactive: true,
            cancellationToken);

        var byId = currencies.ToDictionary(currency => currency.Id);

        return rates.Select(rate =>
        {
            if (!byId.TryGetValue(rate.BaseCurrencyId, out var baseCurrency) ||
                !byId.TryGetValue(rate.QuoteCurrencyId, out var quoteCurrency))
            {
                throw new InvalidOperationException(
                    "Exchange rate references missing currencies.");
            }

            return MapRate(rate, baseCurrency, quoteCurrency);
        }).ToArray();
    }

    private static CurrencyResponse MapCurrency(Currency currency) =>
        new(
            currency.Id,
            currency.OrganizationId,
            currency.Code,
            currency.Name,
            currency.Symbol,
            currency.DecimalPlaces,
            currency.IsBaseCurrency,
            currency.IsActive,
            currency.CreatedAtUtc,
            currency.CreatedByUserId,
            currency.UpdatedAtUtc,
            currency.UpdatedByUserId);

    private static ExchangeRateResponse MapRate(
        ExchangeRate rate,
        Currency baseCurrency,
        Currency quoteCurrency) =>
        new(
            rate.Id,
            rate.OrganizationId,
            rate.BaseCurrencyId,
            baseCurrency.Code,
            rate.QuoteCurrencyId,
            quoteCurrency.Code,
            rate.EffectiveDate,
            rate.Rate,
            rate.Source,
            rate.CreatedAtUtc,
            rate.CreatedByUserId,
            rate.UpdatedAtUtc,
            rate.UpdatedByUserId);

    private static string NormalizeCurrencyCode(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 3 ||
            !normalized.All(char.IsAsciiLetter))
        {
            throw new CurrencyQueryException(
                "Currency code must contain exactly three ASCII letters.");
        }

        return normalized;
    }

    private static void ValidateCurrencyPair(
        Guid baseCurrencyId,
        Guid quoteCurrencyId)
    {
        if (baseCurrencyId == Guid.Empty ||
            quoteCurrencyId == Guid.Empty)
        {
            throw new CurrencyQueryException(
                "Both currency ids are required.");
        }

        if (baseCurrencyId == quoteCurrencyId)
        {
            throw new CurrencyQueryException(
                "Base and quote currencies must be different.");
        }
    }

    private static void ValidateRange(
        DateOnly? fromDate,
        DateOnly? toDate)
    {
        if (fromDate.HasValue &&
            toDate.HasValue &&
            fromDate.Value > toDate.Value)
        {
            throw new CurrencyQueryException(
                "fromDate cannot be after toDate.");
        }
    }
}
