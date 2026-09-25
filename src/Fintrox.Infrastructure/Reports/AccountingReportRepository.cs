using Fintrox.Application.Reports;
using Fintrox.Domain.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Reports;

public sealed class AccountingReportRepository(
    FintroxDbContext dbContext) : IAccountingReportRepository
{
    public async Task<IReadOnlyList<AccountBalanceMovement>>
        GetTrialBalanceMovementsAsync(
            Guid organizationId,
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken)
    {
        return await (
                from line in dbContext.JournalLines.AsNoTracking()
                join entry in dbContext.JournalEntries.AsNoTracking()
                    on new
                    {
                        line.JournalEntryId,
                        line.OrganizationId
                    }
                    equals new
                    {
                        JournalEntryId = entry.Id,
                        entry.OrganizationId
                    }
                where
                    line.OrganizationId == organizationId &&
                    entry.PostingDate <= toDate &&
                    (entry.Status == JournalEntryStatus.Posted ||
                     entry.Status == JournalEntryStatus.Reversed)
                group new
                {
                    line,
                    entry
                }
                by line.AccountId
                into accountMovements
                select new AccountBalanceMovement(
                    accountMovements.Key,
                    accountMovements.Sum(item =>
                        item.entry.PostingDate < fromDate
                            ? item.line.Debit
                            : 0m),
                    accountMovements.Sum(item =>
                        item.entry.PostingDate < fromDate
                            ? item.line.Credit
                            : 0m),
                    accountMovements.Sum(item =>
                        item.entry.PostingDate >= fromDate
                            ? item.line.Debit
                            : 0m),
                    accountMovements.Sum(item =>
                        item.entry.PostingDate >= fromDate
                            ? item.line.Credit
                            : 0m)))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<decimal> GetOpeningNetBalanceAsync(
        Guid organizationId,
        Guid accountId,
        DateOnly fromDate,
        CancellationToken cancellationToken)
    {
        var net = await (
                from line in dbContext.JournalLines.AsNoTracking()
                join entry in dbContext.JournalEntries.AsNoTracking()
                    on new
                    {
                        line.JournalEntryId,
                        line.OrganizationId
                    }
                    equals new
                    {
                        JournalEntryId = entry.Id,
                        entry.OrganizationId
                    }
                where
                    line.OrganizationId == organizationId &&
                    line.AccountId == accountId &&
                    entry.PostingDate < fromDate &&
                    (entry.Status == JournalEntryStatus.Posted ||
                     entry.Status == JournalEntryStatus.Reversed)
                select (decimal?)(line.Debit - line.Credit))
            .SumAsync(cancellationToken);

        return net ?? 0m;
    }

    public async Task<IReadOnlyList<GeneralLedgerMovement>>
        GetGeneralLedgerMovementsAsync(
            Guid organizationId,
            Guid accountId,
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken)
    {
        return await (
                from line in dbContext.JournalLines.AsNoTracking()
                join entry in dbContext.JournalEntries.AsNoTracking()
                    on new
                    {
                        line.JournalEntryId,
                        line.OrganizationId
                    }
                    equals new
                    {
                        JournalEntryId = entry.Id,
                        entry.OrganizationId
                    }
                where
                    line.OrganizationId == organizationId &&
                    line.AccountId == accountId &&
                    entry.PostingDate >= fromDate &&
                    entry.PostingDate <= toDate &&
                    (entry.Status == JournalEntryStatus.Posted ||
                     entry.Status == JournalEntryStatus.Reversed)
                orderby
                    entry.PostingDate,
                    entry.Number,
                    line.LineNumber
                select new GeneralLedgerMovement(
                    entry.Id,
                    entry.Number!,
                    entry.PostingDate,
                    entry.DocumentDate,
                    entry.Description,
                    line.LineNumber,
                    line.Description,
                    entry.Source,
                    entry.ExternalReference,
                    line.Debit,
                    line.Credit))
            .ToArrayAsync(cancellationToken);
    }
}
