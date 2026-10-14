using System.Globalization;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

public sealed class JournalService(
    IJournalRepository journalRepository,
    IAccountRepository accountRepository,
    IFiscalCalendarRepository fiscalCalendarRepository,
    ITransactionRunner transactionRunner,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IJournalService
{
    public async Task<IReadOnlyList<JournalEntrySummaryResponse>> ListAsync(
        DateOnly? fromPostingDate,
        DateOnly? toPostingDate,
        string? status,
        string? source,
        CancellationToken cancellationToken)
    {
        if (fromPostingDate.HasValue &&
            toPostingDate.HasValue &&
            fromPostingDate.Value > toPostingDate.Value)
        {
            throw new JournalQueryException(
                "The from posting date cannot be after the to posting date.");
        }

        var parsedStatus = ParseStatusFilter(status);
        var parsedSource = ParseSourceFilter(source);
        var organizationId = currentOrganization.RequireOrganizationId();

        var entries = await journalRepository.ListEntriesAsync(
            organizationId,
            fromPostingDate,
            toPostingDate,
            parsedStatus,
            parsedSource,
            cancellationToken);

        return entries.Select(MapSummary).ToArray();
    }

    public async Task<JournalEntryResponse?> GetAsync(
        Guid journalEntryId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var entry = await journalRepository.GetEntryAsync(
            organizationId,
            journalEntryId,
            trackChanges: false,
            cancellationToken);

        return entry is null
            ? null
            : await BuildResponseAsync(entry, cancellationToken);
    }

    public async Task<JournalEntryResponse> CreateAsync(
        CreateJournalEntryRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var period = await fiscalCalendarRepository.FindPeriodByDateAsync(
            organizationId,
            request.PostingDate,
            trackChanges: false,
            cancellationToken);

        if (period is null)
        {
            throw new JournalConflictException(
                "No accounting period contains the requested posting date.");
        }

        var now = timeProvider.GetUtcNow();
        var entry = JournalEntry.CreateDraft(
            organizationId,
            period.Id,
            request.PostingDate,
            request.DocumentDate,
            request.Description,
            JournalEntrySource.Manual,
            request.ExternalReference,
            now);

        await journalRepository.AddEntryAsync(entry, cancellationToken);

        var lineNumber = 1;

        foreach (var requestLine in request.Lines ?? [])
        {
            await ValidateManualLineAsync(
                organizationId,
                requestLine.AccountId,
                requestLine.Debit,
                requestLine.Credit,
                cancellationToken);

            var line = JournalLine.Create(
                organizationId,
                entry.Id,
                lineNumber++,
                requestLine.AccountId,
                requestLine.Debit,
                requestLine.Credit,
                requestLine.Description,
                now);

            await journalRepository.AddLineAsync(line, cancellationToken);
        }

        await journalRepository.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(entry, cancellationToken);
    }

    /// <summary>
    /// Imports one balanced opening GL position on the fiscal year's first
    /// day. A year with existing posted ledger activity cannot be initialized
    /// retroactively. The year lock serializes this gate with normal posting.
    /// </summary>
    public async Task<JournalEntryResponse> ImportOpeningBalancesAsync(
        ImportOpeningBalancesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var lines = request.Lines ?? [];
        if (request.FiscalYearId == Guid.Empty ||
            lines.Count is < 2 or > 1000)
        {
            throw new JournalConflictException(
                "An opening-balance import requires a fiscal year and 2 to 1000 lines.");
        }

        foreach (var line in lines)
        {
            ValidateLineAmounts(line.Debit, line.Credit);
            if (line.AccountId == Guid.Empty)
            {
                throw new JournalConflictException(
                    "Every opening-balance line must reference an account.");
            }
        }

        var accountIds = lines.Select(line => line.AccountId).Distinct().ToArray();
        if (accountIds.Length != lines.Count)
        {
            throw new JournalConflictException(
                "An opening-balance import must contain each account exactly once.");
        }

        var debit = lines.Sum(line => line.Debit);
        var credit = lines.Sum(line => line.Credit);
        if (debit <= 0m || debit != credit)
        {
            throw new JournalConflictException(
                "Opening balances must have equal positive debit and credit totals.");
        }

        var organizationId = currentOrganization.RequireOrganizationId();
        var result = await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                await journalRepository.LockFiscalYearForOpeningBalancesAsync(
                    organizationId,
                    request.FiscalYearId,
                    transactionCancellationToken);

                // Every authorization, account and fiscal check is repeated
                // after acquiring the PostgreSQL lock, not based on stale data.
                var year = await fiscalCalendarRepository.GetFiscalYearAsync(
                    organizationId,
                    request.FiscalYearId,
                    trackChanges: false,
                    transactionCancellationToken);

                if (year is null || year.Status != FiscalYearStatus.Open)
                {
                    throw new JournalConflictException(
                        "The fiscal year does not exist or is not open.");
                }

                var (period, fiscalYear) = await ResolveOpenPostingContextAsync(
                    organizationId,
                    year.StartDate,
                    expectedPeriodId: null,
                    transactionCancellationToken);

                if (period.FiscalYearId != request.FiscalYearId)
                {
                    throw new JournalConflictException(
                        "The fiscal year's first day is not covered by one of its open periods.");
                }

                var reference = $"opening-balance:v1:{request.FiscalYearId:N}";
                var existing = await journalRepository.GetSystemEntryByExternalReferenceAsync(
                    organizationId,
                    reference,
                    trackChanges: false,
                    transactionCancellationToken);

                if (existing is not null)
                {
                    var existingLines = await journalRepository.ListLinesAsync(
                        organizationId,
                        existing.Id,
                        transactionCancellationToken);

                    if (existing.Status != JournalEntryStatus.Posted ||
                        existing.PostingDate != fiscalYear.StartDate ||
                        existingLines.Count != lines.Count ||
                        existingLines.Where((line, index) =>
                            line.AccountId != lines[index].AccountId ||
                            line.Debit != lines[index].Debit ||
                            line.Credit != lines[index].Credit ||
                            line.Description != NormalizeOpeningDescription(
                                lines[index].Description)).Any())
                    {
                        throw new JournalConflictException(
                            "Opening balances for this fiscal year already exist with different values or status.");
                    }

                    return existing;
                }

                if (await journalRepository.HasPostedEntriesForFiscalYearAsync(
                        organizationId,
                        request.FiscalYearId,
                        transactionCancellationToken))
                {
                    throw new JournalConflictException(
                        "Opening balances cannot be imported after ledger activity has been posted in this fiscal year.");
                }

                var accounts = await accountRepository.ListByIdsAsync(
                    organizationId,
                    accountIds,
                    transactionCancellationToken);

                if (accounts.Count != accountIds.Length ||
                    accounts.Values.Any(account =>
                        !account.IsActive || !account.AllowManualPosting))
                {
                    throw new JournalConflictException(
                        "Opening balances require active, manually postable accounts owned by this organization.");
                }

                var now = timeProvider.GetUtcNow();
                var entry = JournalEntry.CreateDraft(
                    organizationId,
                    period.Id,
                    fiscalYear.StartDate,
                    fiscalYear.StartDate,
                    $"Opening balances for fiscal year {fiscalYear.StartDate:yyyy-MM-dd}",
                    JournalEntrySource.System,
                    reference,
                    now);

                await journalRepository.AddEntryAsync(entry, transactionCancellationToken);
                for (var index = 0; index < lines.Count; index++)
                {
                    var item = lines[index];
                    await journalRepository.AddLineAsync(
                        JournalLine.Create(
                            organizationId,
                            entry.Id,
                            index + 1,
                            item.AccountId,
                            item.Debit,
                            item.Credit,
                            item.Description,
                            now),
                        transactionCancellationToken);
                }

                // Persist as draft first: PostgreSQL's journal immutability
                // trigger forbids inserting lines under a posted parent.
                await journalRepository.SaveChangesAsync(transactionCancellationToken);

                var sequence = await journalRepository.AllocatePostingSequenceAsync(
                    organizationId,
                    period.FiscalYearId,
                    transactionCancellationToken);

                entry.Post(
                    FormatJournalNumber(fiscalYear.StartDate, sequence),
                    now);
                await journalRepository.SaveChangesAsync(transactionCancellationToken);

                return entry;
            },
            cancellationToken);

        return await BuildResponseAsync(result, cancellationToken);
    }

    private static string? NormalizeOpeningDescription(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public async Task<JournalEntryResponse?> UpdateAsync(
        Guid journalEntryId,
        UpdateJournalEntryRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var entry = await journalRepository.GetEntryAsync(
            organizationId,
            journalEntryId,
            trackChanges: true,
            cancellationToken);

        if (entry is null)
        {
            return null;
        }

        EnsureDraft(entry);

        var period = await fiscalCalendarRepository.FindPeriodByDateAsync(
            organizationId,
            request.PostingDate,
            trackChanges: false,
            cancellationToken);

        if (period is null)
        {
            throw new JournalConflictException(
                "No accounting period contains the requested posting date.");
        }

        entry.UpdateDraft(
            period.Id,
            request.PostingDate,
            request.DocumentDate,
            request.Description,
            request.ExternalReference,
            timeProvider.GetUtcNow());

        await journalRepository.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(entry, cancellationToken);
    }

    public async Task<JournalLineResponse?> AddLineAsync(
        Guid journalEntryId,
        CreateJournalLineRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var entry = await journalRepository.GetEntryAsync(
            organizationId,
            journalEntryId,
            trackChanges: false,
            cancellationToken);

        if (entry is null)
        {
            return null;
        }

        EnsureDraft(entry);

        var account = await ValidateManualLineAsync(
            organizationId,
            request.AccountId,
            request.Debit,
            request.Credit,
            cancellationToken);

        var lineNumber = await journalRepository.GetNextLineNumberAsync(
            organizationId,
            journalEntryId,
            cancellationToken);

        var line = JournalLine.Create(
            organizationId,
            journalEntryId,
            lineNumber,
            request.AccountId,
            request.Debit,
            request.Credit,
            request.Description,
            timeProvider.GetUtcNow());

        await journalRepository.AddLineAsync(line, cancellationToken);
        await journalRepository.SaveChangesAsync(cancellationToken);

        return MapLine(line, account);
    }

    public async Task<JournalLineResponse?> UpdateLineAsync(
        Guid journalEntryId,
        Guid journalLineId,
        UpdateJournalLineRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var entry = await journalRepository.GetEntryAsync(
            organizationId,
            journalEntryId,
            trackChanges: false,
            cancellationToken);

        if (entry is null)
        {
            return null;
        }

        EnsureDraft(entry);

        var line = await journalRepository.GetLineAsync(
            organizationId,
            journalEntryId,
            journalLineId,
            trackChanges: true,
            cancellationToken);

        if (line is null)
        {
            return null;
        }

        var account = await ValidateManualLineAsync(
            organizationId,
            request.AccountId,
            request.Debit,
            request.Credit,
            cancellationToken);

        line.Update(
            request.AccountId,
            request.Debit,
            request.Credit,
            request.Description,
            timeProvider.GetUtcNow());

        await journalRepository.SaveChangesAsync(cancellationToken);
        return MapLine(line, account);
    }

    public async Task<bool> DeleteLineAsync(
        Guid journalEntryId,
        Guid journalLineId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var entry = await journalRepository.GetEntryAsync(
            organizationId,
            journalEntryId,
            trackChanges: false,
            cancellationToken);

        if (entry is null)
        {
            return false;
        }

        EnsureDraft(entry);

        var line = await journalRepository.GetLineAsync(
            organizationId,
            journalEntryId,
            journalLineId,
            trackChanges: true,
            cancellationToken);

        if (line is null)
        {
            return false;
        }

        journalRepository.RemoveLine(line);
        await journalRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<JournalEntryResponse?> PostAsync(
        Guid journalEntryId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var postedEntry = await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                await journalRepository.LockJournalForPostingAsync(
                    organizationId,
                    journalEntryId,
                    transactionCancellationToken);

                var entry = await journalRepository.GetEntryAsync(
                    organizationId,
                    journalEntryId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (entry is null)
                {
                    return null;
                }

                EnsureDraft(entry);

                var (period, fiscalYear) = await ResolveOpenPostingContextAsync(
                    organizationId,
                    entry.PostingDate,
                    entry.FiscalPeriodId,
                    transactionCancellationToken);

                var lines = await journalRepository.ListLinesAsync(
                    organizationId,
                    entry.Id,
                    transactionCancellationToken);

                await ValidatePostingLinesAsync(
                    entry,
                    lines,
                    transactionCancellationToken);

                var sequence =
                    await journalRepository.AllocatePostingSequenceAsync(
                        organizationId,
                        period.FiscalYearId,
                        transactionCancellationToken);

                entry.Post(
                    FormatJournalNumber(
                        fiscalYear.StartDate,
                        sequence),
                    timeProvider.GetUtcNow());

                await journalRepository.SaveChangesAsync(
                    transactionCancellationToken);

                return entry;
            },
            cancellationToken);

        return postedEntry is null
            ? null
            : await BuildResponseAsync(postedEntry, cancellationToken);
    }

    public async Task<JournalEntryResponse?> ReverseAsync(
        Guid journalEntryId,
        ReverseJournalEntryRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var reversalEntry = await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                await journalRepository.LockJournalForPostingAsync(
                    organizationId,
                    journalEntryId,
                    transactionCancellationToken);

                var original = await journalRepository.GetEntryAsync(
                    organizationId,
                    journalEntryId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (original is null)
                {
                    return null;
                }

                EnsureReversible(original);

                var (period, fiscalYear) = await ResolveOpenPostingContextAsync(
                    organizationId,
                    request.PostingDate,
                    expectedPeriodId: null,
                    transactionCancellationToken);

                var originalLines = await journalRepository.ListLinesAsync(
                    organizationId,
                    original.Id,
                    transactionCancellationToken);

                if (originalLines.Count < 2)
                {
                    throw new JournalConflictException(
                        "The posted journal entry does not contain enough lines to reverse.");
                }

                var accountIds = originalLines
                    .Select(line => line.AccountId)
                    .Distinct()
                    .ToArray();

                var accounts = await accountRepository.ListByIdsAsync(
                    organizationId,
                    accountIds,
                    transactionCancellationToken);

                if (accounts.Count != accountIds.Length)
                {
                    throw new JournalConflictException(
                        "The posted journal entry references an account that no longer exists.");
                }

                var now = timeProvider.GetUtcNow();
                var reversal = JournalEntry.CreateReversalDraft(
                    organizationId,
                    period.Id,
                    original.Id,
                    request.PostingDate,
                    BuildReversalDescription(
                        original.Number,
                        request.Reason),
                    now);

                await journalRepository.AddEntryAsync(
                    reversal,
                    transactionCancellationToken);

                foreach (var originalLine in originalLines)
                {
                    var reversalLine = JournalLine.Create(
                        organizationId,
                        reversal.Id,
                        originalLine.LineNumber,
                        originalLine.AccountId,
                        originalLine.Credit,
                        originalLine.Debit,
                        originalLine.Description,
                        now);

                    await journalRepository.AddLineAsync(
                        reversalLine,
                        transactionCancellationToken);
                }

                // Persist the reversal as a draft first so database-level
                // immutability guards can verify that its lines are inserted
                // only while the parent entry is editable.
                await journalRepository.SaveChangesAsync(
                    transactionCancellationToken);

                var sequence =
                    await journalRepository.AllocatePostingSequenceAsync(
                        organizationId,
                        period.FiscalYearId,
                        transactionCancellationToken);

                reversal.Post(
                    FormatJournalNumber(
                        fiscalYear.StartDate,
                        sequence),
                    now);

                original.MarkReversed(reversal.Id, now);

                await journalRepository.SaveChangesAsync(
                    transactionCancellationToken);

                return reversal;
            },
            cancellationToken);

        return reversalEntry is null
            ? null
            : await BuildResponseAsync(
                reversalEntry,
                cancellationToken);
    }

    private async Task<Account> ValidateManualLineAsync(
        Guid organizationId,
        Guid accountId,
        decimal debit,
        decimal credit,
        CancellationToken cancellationToken)
    {
        ValidateLineAmounts(debit, credit);

        if (accountId == Guid.Empty)
        {
            throw new JournalConflictException(
                "A journal line must reference an account.");
        }

        var account = await accountRepository.GetAsync(
            organizationId,
            accountId,
            trackChanges: false,
            cancellationToken);

        if (account is null)
        {
            throw new JournalConflictException(
                "The selected account does not exist in this organization.");
        }

        if (!account.IsActive)
        {
            throw new JournalConflictException(
                $"Account '{account.Code}' is inactive.");
        }

        if (!account.AllowManualPosting)
        {
            throw new JournalConflictException(
                $"Account '{account.Code}' does not allow manual posting.");
        }

        return account;
    }

    private async Task ValidatePostingLinesAsync(
        JournalEntry entry,
        IReadOnlyList<JournalLine> lines,
        CancellationToken cancellationToken)
    {
        if (lines.Count < 2)
        {
            throw new JournalConflictException(
                "A journal entry must contain at least two lines before posting.");
        }

        var debitTotal = lines.Sum(line => line.Debit);
        var creditTotal = lines.Sum(line => line.Credit);

        if (debitTotal <= 0m || debitTotal != creditTotal)
        {
            throw new JournalConflictException(
                "The journal entry must be balanced before posting.");
        }

        var accountIds = lines
            .Select(line => line.AccountId)
            .Distinct()
            .ToArray();

        var accounts = await accountRepository.ListByIdsAsync(
            entry.OrganizationId,
            accountIds,
            cancellationToken);

        if (accounts.Count != accountIds.Length)
        {
            throw new JournalConflictException(
                "A journal line references an account that does not exist.");
        }

        foreach (var account in accounts.Values)
        {
            if (!account.IsActive)
            {
                throw new JournalConflictException(
                    $"Account '{account.Code}' is inactive.");
            }

            if (entry.Source == JournalEntrySource.Manual &&
                !account.AllowManualPosting)
            {
                throw new JournalConflictException(
                    $"Account '{account.Code}' does not allow manual posting.");
            }
        }
    }

    private async Task<(AccountingPeriod Period, FiscalYear FiscalYear)>
        ResolveOpenPostingContextAsync(
            Guid organizationId,
            DateOnly postingDate,
            Guid? expectedPeriodId,
            CancellationToken cancellationToken)
    {
        var initialPeriod = await fiscalCalendarRepository.FindPeriodByDateAsync(
            organizationId,
            postingDate,
            trackChanges: false,
            cancellationToken);

        if (initialPeriod is null)
        {
            throw new JournalConflictException(
                "No accounting period contains the requested posting date.");
        }

        if (expectedPeriodId.HasValue &&
            initialPeriod.Id != expectedPeriodId.Value)
        {
            throw new JournalConflictException(
                "The journal entry fiscal period does not match its posting date.");
        }

        // The first read only locates the rows; its status is not trusted.
        // Lock year then period inside the caller's transaction, and re-read
        // both AFTER waiting for any concurrent close/reopen to commit.
        await journalRepository.LockPostingCalendarAsync(
            organizationId,
            initialPeriod.FiscalYearId,
            initialPeriod.Id,
            cancellationToken);

        var period = await fiscalCalendarRepository.FindPeriodByDateAsync(
            organizationId,
            postingDate,
            trackChanges: false,
            cancellationToken);

        if (period is null ||
            period.Id != initialPeriod.Id ||
            period.FiscalYearId != initialPeriod.FiscalYearId)
        {
            throw new JournalConflictException(
                "The accounting period changed during posting. Retry after reviewing the fiscal calendar.");
        }

        if (period.Status != AccountingPeriodStatus.Open)
        {
            throw new JournalConflictException(
                "Posting is allowed only in an open accounting period.");
        }

        var fiscalYear = await fiscalCalendarRepository.GetFiscalYearAsync(
            organizationId,
            period.FiscalYearId,
            trackChanges: false,
            cancellationToken);

        if (fiscalYear is null)
        {
            throw new JournalConflictException(
                "The accounting period does not belong to a valid fiscal year.");
        }

        if (fiscalYear.Status != FiscalYearStatus.Open)
        {
            throw new JournalConflictException(
                "Posting is allowed only in an open fiscal year.");
        }

        return (period, fiscalYear);
    }

    private async Task<JournalEntryResponse> BuildResponseAsync(
        JournalEntry entry,
        CancellationToken cancellationToken)
    {
        var lines = await journalRepository.ListLinesAsync(
            entry.OrganizationId,
            entry.Id,
            cancellationToken);

        var accountIds = lines
            .Select(line => line.AccountId)
            .Distinct()
            .ToArray();

        var accounts = await accountRepository.ListByIdsAsync(
            entry.OrganizationId,
            accountIds,
            cancellationToken);

        var lineResponses = lines
            .OrderBy(line => line.LineNumber)
            .Select(line =>
            {
                if (!accounts.TryGetValue(line.AccountId, out var account))
                {
                    throw new InvalidOperationException(
                        "A journal line references a missing account.");
                }

                return MapLine(line, account);
            })
            .ToArray();

        var debitTotal = lines.Sum(line => line.Debit);
        var creditTotal = lines.Sum(line => line.Credit);

        return new JournalEntryResponse(
            entry.Id,
            entry.OrganizationId,
            entry.Number,
            entry.PostingDate,
            entry.DocumentDate,
            entry.Description,
            entry.Status.ToString(),
            entry.Source.ToString(),
            entry.ExternalReference,
            entry.FiscalPeriodId,
            entry.PostedAtUtc,
            entry.ReversalOfJournalEntryId,
            entry.ReversedByJournalEntryId,
            debitTotal,
            creditTotal,
            lines.Count >= 2 &&
            debitTotal > 0m &&
            debitTotal == creditTotal,
            lineResponses,
            entry.CreatedAtUtc,
            entry.CreatedByUserId,
            entry.UpdatedAtUtc,
            entry.UpdatedByUserId);
    }

    private static JournalLineResponse MapLine(
        JournalLine line,
        Account account)
    {
        return new JournalLineResponse(
            line.Id,
            line.LineNumber,
            line.AccountId,
            account.Code,
            account.Name,
            line.Debit,
            line.Credit,
            line.Description,
            line.CreatedAtUtc,
            line.CreatedByUserId,
            line.UpdatedAtUtc,
            line.UpdatedByUserId);
    }

    private static JournalEntrySummaryResponse MapSummary(JournalEntry entry)
    {
        return new JournalEntrySummaryResponse(
            entry.Id,
            entry.Number,
            entry.PostingDate,
            entry.DocumentDate,
            entry.Description,
            entry.Status.ToString(),
            entry.Source.ToString(),
            entry.ExternalReference,
            entry.FiscalPeriodId,
            entry.PostedAtUtc,
            entry.ReversalOfJournalEntryId,
            entry.ReversedByJournalEntryId,
            entry.CreatedAtUtc,
            entry.UpdatedAtUtc);
    }

    private static JournalEntryStatus? ParseStatusFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Enum.TryParse<JournalEntryStatus>(
                value,
                ignoreCase: true,
                out var status) ||
            !Enum.IsDefined(status))
        {
            throw new JournalQueryException(
                $"Unknown journal status '{value}'.");
        }

        return status;
    }

    private static JournalEntrySource? ParseSourceFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Enum.TryParse<JournalEntrySource>(
                value,
                ignoreCase: true,
                out var source) ||
            !Enum.IsDefined(source))
        {
            throw new JournalQueryException(
                $"Unknown journal source '{value}'.");
        }

        return source;
    }

    private static void EnsureDraft(JournalEntry entry)
    {
        if (entry.Status != JournalEntryStatus.Draft)
        {
            throw new JournalConflictException(
                "Only draft journal entries can be edited.");
        }
    }

    private static void EnsureReversible(JournalEntry entry)
    {
        if (entry.Status == JournalEntryStatus.Reversed ||
            entry.ReversedByJournalEntryId is not null)
        {
            throw new JournalConflictException(
                "This journal entry has already been reversed.");
        }

        if (entry.Status != JournalEntryStatus.Posted)
        {
            throw new JournalConflictException(
                "Only a posted journal entry can be reversed.");
        }
    }

    private static void ValidateLineAmounts(decimal debit, decimal credit)
    {
        if (debit < 0m || credit < 0m)
        {
            throw new JournalConflictException(
                "Debit and credit amounts cannot be negative.");
        }

        if ((debit == 0m && credit == 0m) ||
            (debit > 0m && credit > 0m))
        {
            throw new JournalConflictException(
                "A journal line must contain a positive amount on exactly one side.");
        }

        if (decimal.Round(debit, 4) != debit ||
            decimal.Round(credit, 4) != credit)
        {
            throw new JournalConflictException(
                "Journal amounts support at most four decimal places.");
        }
    }

    private static string FormatJournalNumber(
        DateOnly fiscalYearStartDate,
        long sequence)
    {
        if (sequence <= 0)
        {
            throw new InvalidOperationException(
                "Journal posting sequence must be positive.");
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{fiscalYearStartDate:yyyyMMdd}-{sequence:D6}");
    }

    private static string BuildReversalDescription(
        string? originalNumber,
        string reason)
    {
        var normalizedReason = reason.Trim();
        var sourceNumber = string.IsNullOrWhiteSpace(originalNumber)
            ? "un-numbered entry"
            : originalNumber;

        return $"Reversal of {sourceNumber}: {normalizedReason}";
    }
}
