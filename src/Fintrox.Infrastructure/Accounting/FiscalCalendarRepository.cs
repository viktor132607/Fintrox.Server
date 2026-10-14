using Fintrox.Application.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Accounting;

public sealed class FiscalCalendarRepository(
    FintroxDbContext dbContext) : IFiscalCalendarRepository
{
    public async Task<IReadOnlyList<FiscalYear>> ListFiscalYearsAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.FiscalYears
            .AsNoTracking()
            .Where(year => year.OrganizationId == organizationId)
            .OrderByDescending(year => year.StartDate)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<FiscalYear?> GetFiscalYearAsync(
        Guid organizationId,
        Guid fiscalYearId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<FiscalYear> query = dbContext.FiscalYears;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            year =>
                year.OrganizationId == organizationId &&
                year.Id == fiscalYearId,
            cancellationToken);
    }

    public Task<bool> FiscalYearNameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludingFiscalYearId,
        CancellationToken cancellationToken)
    {
        return dbContext.FiscalYears.AnyAsync(
            year =>
                year.OrganizationId == organizationId &&
                year.Name == name &&
                (!excludingFiscalYearId.HasValue ||
                 year.Id != excludingFiscalYearId.Value),
            cancellationToken);
    }

    public Task<bool> FiscalYearRangeOverlapsAsync(
        Guid organizationId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludingFiscalYearId,
        CancellationToken cancellationToken)
    {
        return dbContext.FiscalYears.AnyAsync(
            year =>
                year.OrganizationId == organizationId &&
                year.StartDate <= endDate &&
                year.EndDate >= startDate &&
                (!excludingFiscalYearId.HasValue ||
                 year.Id != excludingFiscalYearId.Value),
            cancellationToken);
    }

    public Task<bool> HasPeriodOutsideRangeAsync(
        Guid organizationId,
        Guid fiscalYearId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        return dbContext.AccountingPeriods.AnyAsync(
            period =>
                period.OrganizationId == organizationId &&
                period.FiscalYearId == fiscalYearId &&
                (period.StartDate < startDate ||
                 period.EndDate > endDate),
            cancellationToken);
    }

    public async Task<IReadOnlyList<AccountingPeriod>> ListPeriodsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return await dbContext.AccountingPeriods
            .AsNoTracking()
            .Where(period =>
                period.OrganizationId == organizationId &&
                period.FiscalYearId == fiscalYearId)
            .OrderBy(period => period.Number)
            .ThenBy(period => period.StartDate)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AccountingPeriod?> GetPeriodAsync(
        Guid organizationId,
        Guid fiscalYearId,
        Guid periodId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<AccountingPeriod> query = dbContext.AccountingPeriods;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            period =>
                period.OrganizationId == organizationId &&
                period.FiscalYearId == fiscalYearId &&
                period.Id == periodId,
            cancellationToken);
    }

    public async Task<AccountingPeriod?> FindPeriodByDateAsync(
        Guid organizationId,
        DateOnly postingDate,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<AccountingPeriod> query = dbContext.AccountingPeriods;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            period =>
                period.OrganizationId == organizationId &&
                period.StartDate <= postingDate &&
                period.EndDate >= postingDate,
            cancellationToken);
    }

    public Task<bool> PeriodNumberExistsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        int number,
        Guid? excludingPeriodId,
        CancellationToken cancellationToken)
    {
        return dbContext.AccountingPeriods.AnyAsync(
            period =>
                period.OrganizationId == organizationId &&
                period.FiscalYearId == fiscalYearId &&
                period.Number == number &&
                (!excludingPeriodId.HasValue ||
                 period.Id != excludingPeriodId.Value),
            cancellationToken);
    }

    public Task<bool> PeriodRangeOverlapsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludingPeriodId,
        CancellationToken cancellationToken)
    {
        return dbContext.AccountingPeriods.AnyAsync(
            period =>
                period.OrganizationId == organizationId &&
                period.FiscalYearId == fiscalYearId &&
                period.StartDate <= endDate &&
                period.EndDate >= startDate &&
                (!excludingPeriodId.HasValue ||
                 period.Id != excludingPeriodId.Value),
            cancellationToken);
    }

    public async Task LockFiscalYearForCloseAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        EnsureCloseTransaction();
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            SELECT id FROM accounting.fiscal_years
            WHERE organization_id = {organizationId} AND id = {fiscalYearId}
            FOR UPDATE
            """,
            cancellationToken);
    }

    public async Task LockAccountingPeriodForCloseAsync(
        Guid organizationId,
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        EnsureCloseTransaction();
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            SELECT id FROM accounting.accounting_periods
            WHERE organization_id = {organizationId}
              AND fiscal_year_id = {fiscalYearId}
              AND id = {periodId}
            FOR UPDATE
            """,
            cancellationToken);
    }

    public Task<bool> HasDraftJournalsInPeriodAsync(
        Guid organizationId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        return dbContext.JournalEntries.AsNoTracking().AnyAsync(
            entry => entry.OrganizationId == organizationId &&
                entry.FiscalPeriodId == periodId &&
                entry.Status == JournalEntryStatus.Draft,
            cancellationToken);
    }

    public Task<bool> HasDraftJournalsInFiscalYearAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return dbContext.JournalEntries.AsNoTracking().AnyAsync(
            entry => entry.OrganizationId == organizationId &&
                entry.Status == JournalEntryStatus.Draft &&
                dbContext.AccountingPeriods.Any(period =>
                    period.OrganizationId == organizationId &&
                    period.Id == entry.FiscalPeriodId &&
                    period.FiscalYearId == fiscalYearId),
            cancellationToken);
    }

    public Task<bool> HasMisdatedJournalsInPeriodAsync(
        Guid organizationId,
        Guid periodId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        return dbContext.JournalEntries.AsNoTracking().AnyAsync(
            entry => entry.OrganizationId == organizationId &&
                entry.FiscalPeriodId == periodId &&
                (entry.PostingDate < startDate || entry.PostingDate > endDate),
            cancellationToken);
    }

    private void EnsureCloseTransaction()
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Fiscal close row locks require an active transaction.");
        }
    }

    public Task<int> CountPeriodsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return dbContext.AccountingPeriods.CountAsync(
            period =>
                period.OrganizationId == organizationId &&
                period.FiscalYearId == fiscalYearId,
            cancellationToken);
    }

    public Task<bool> HasOpenPeriodsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return dbContext.AccountingPeriods.AnyAsync(
            period =>
                period.OrganizationId == organizationId &&
                period.FiscalYearId == fiscalYearId &&
                period.Status == AccountingPeriodStatus.Open,
            cancellationToken);
    }

    public Task<bool> AllPeriodsClosedAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return dbContext.AccountingPeriods.AllAsync(
            period =>
                period.OrganizationId != organizationId ||
                period.FiscalYearId != fiscalYearId ||
                period.Status == AccountingPeriodStatus.Closed,
            cancellationToken);
    }

    public async Task AddFiscalYearAsync(
        FiscalYear fiscalYear,
        CancellationToken cancellationToken)
    {
        await dbContext.FiscalYears.AddAsync(
            fiscalYear,
            cancellationToken);
    }

    public async Task AddPeriodAsync(
        AccountingPeriod period,
        CancellationToken cancellationToken)
    {
        await dbContext.AccountingPeriods.AddAsync(
            period,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
