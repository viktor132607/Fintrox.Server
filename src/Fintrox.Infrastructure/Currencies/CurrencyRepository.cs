using Fintrox.Application.Currencies;
using Fintrox.Domain.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Currencies;

public sealed class CurrencyRepository(
    FintroxDbContext dbContext) : ICurrencyRepository
{
    public async Task<IReadOnlyList<Currency>> ListCurrenciesAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        IQueryable<Currency> query = dbContext.Currencies
            .AsNoTracking()
            .Where(currency => currency.OrganizationId == organizationId);

        if (!includeInactive)
        {
            query = query.Where(currency => currency.IsActive);
        }

        return await query
            .OrderByDescending(currency => currency.IsBaseCurrency)
            .ThenBy(currency => currency.Code)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Currency?> GetCurrencyAsync(
        Guid organizationId,
        Guid currencyId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<Currency> query = dbContext.Currencies;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            currency =>
                currency.OrganizationId == organizationId &&
                currency.Id == currencyId,
            cancellationToken);
    }

    public async Task<Currency?> GetBaseCurrencyAsync(
        Guid organizationId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<Currency> query = dbContext.Currencies;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            currency =>
                currency.OrganizationId == organizationId &&
                currency.IsBaseCurrency,
            cancellationToken);
    }

    public Task<bool> CurrencyCodeExistsAsync(
        Guid organizationId,
        string code,
        Guid? excludingCurrencyId,
        CancellationToken cancellationToken)
    {
        return dbContext.Currencies.AnyAsync(
            currency =>
                currency.OrganizationId == organizationId &&
                currency.Code == code &&
                (!excludingCurrencyId.HasValue ||
                 currency.Id != excludingCurrencyId.Value),
            cancellationToken);
    }

    public async Task AddCurrencyAsync(
        Currency currency,
        CancellationToken cancellationToken)
    {
        await dbContext.Currencies.AddAsync(currency, cancellationToken);
    }

    public async Task<IReadOnlyList<ExchangeRate>> ListExchangeRatesAsync(
        Guid organizationId,
        Guid? baseCurrencyId,
        Guid? quoteCurrencyId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        IQueryable<ExchangeRate> query = dbContext.ExchangeRates
            .AsNoTracking()
            .Where(rate => rate.OrganizationId == organizationId);

        if (baseCurrencyId.HasValue)
        {
            query = query.Where(rate =>
                rate.BaseCurrencyId == baseCurrencyId.Value);
        }

        if (quoteCurrencyId.HasValue)
        {
            query = query.Where(rate =>
                rate.QuoteCurrencyId == quoteCurrencyId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(rate =>
                rate.EffectiveDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(rate =>
                rate.EffectiveDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(rate => rate.EffectiveDate)
            .ThenBy(rate => rate.BaseCurrencyId)
            .ThenBy(rate => rate.QuoteCurrencyId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ExchangeRate?> GetExchangeRateAsync(
        Guid organizationId,
        Guid exchangeRateId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<ExchangeRate> query = dbContext.ExchangeRates;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            rate =>
                rate.OrganizationId == organizationId &&
                rate.Id == exchangeRateId,
            cancellationToken);
    }

    public Task<ExchangeRate?> GetLatestExchangeRateAsync(
        Guid organizationId,
        Guid baseCurrencyId,
        Guid quoteCurrencyId,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        return dbContext.ExchangeRates
            .AsNoTracking()
            .Where(rate =>
                rate.OrganizationId == organizationId &&
                rate.BaseCurrencyId == baseCurrencyId &&
                rate.QuoteCurrencyId == quoteCurrencyId &&
                rate.EffectiveDate <= asOfDate)
            .OrderByDescending(rate => rate.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExchangeRateExistsAsync(
        Guid organizationId,
        Guid baseCurrencyId,
        Guid quoteCurrencyId,
        DateOnly effectiveDate,
        Guid? excludingExchangeRateId,
        CancellationToken cancellationToken)
    {
        return dbContext.ExchangeRates.AnyAsync(
            rate =>
                rate.OrganizationId == organizationId &&
                rate.BaseCurrencyId == baseCurrencyId &&
                rate.QuoteCurrencyId == quoteCurrencyId &&
                rate.EffectiveDate == effectiveDate &&
                (!excludingExchangeRateId.HasValue ||
                 rate.Id != excludingExchangeRateId.Value),
            cancellationToken);
    }

    public async Task AddExchangeRateAsync(
        ExchangeRate exchangeRate,
        CancellationToken cancellationToken)
    {
        await dbContext.ExchangeRates.AddAsync(
            exchangeRate,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
