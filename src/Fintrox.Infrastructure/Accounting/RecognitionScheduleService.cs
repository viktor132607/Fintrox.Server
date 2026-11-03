using Fintrox.Application.Accounting;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// Explicit, immutable monthly accrual/deferral schedule. Each occurrence
/// generates one unposted System journal with tenant-bound audit evidence.
/// No cash movement, automatic posting or source invoice is inferred.
/// </summary>
public sealed class RecognitionScheduleService(
    FintroxDbContext db,
    ICurrentOrganization organization,
    ICurrentUser currentUser,
    IJournalRepository journals,
    IFiscalCalendarRepository calendars,
    TimeProvider clock) : IRecognitionScheduleService
{
    private const string SelectSchedule = """
        SELECT id,kind,name,description,first_posting_date,months,
               total_amount,monthly_amount,debit_account_id,credit_account_id
        FROM accounting.recognition_schedules
        WHERE organization_id=@org
        """;

    public async Task<IReadOnlyList<RecognitionScheduleResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var connection = (NpgsqlConnection)db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                await using var command = new NpgsqlCommand(
                    SelectSchedule + " ORDER BY created_at_utc DESC,id LIMIT 200", connection);
                command.Parameters.AddWithValue("org", tenant);
                var result = new List<RecognitionScheduleResponse>();
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    result.Add(ReadSchedule(reader));
                return (IReadOnlyList<RecognitionScheduleResponse>)result;
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        });
    }

    public async Task<RecognitionScheduleDetailResponse?> GetAsync(
        Guid scheduleId, CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        if (scheduleId == Guid.Empty) return null;
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var connection = (NpgsqlConnection)db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                RecognitionScheduleResponse schedule;
                await using (var command = new NpgsqlCommand(
                    SelectSchedule + " AND id=@id", connection))
                {
                    command.Parameters.AddWithValue("org", tenant);
                    command.Parameters.AddWithValue("id", scheduleId);
                    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                    if (!await reader.ReadAsync(cancellationToken)) return null;
                    schedule = ReadSchedule(reader);
                }

                var generated = new Dictionary<int, (Guid Journal, string Status)>();
                await using (var command = new NpgsqlCommand("""
                    SELECT g.period_number,g.journal_entry_id,e.status
                    FROM accounting.recognition_generations g
                    JOIN accounting.journal_entries e
                      ON e.id=g.journal_entry_id AND e.organization_id=g.organization_id
                    WHERE g.organization_id=@org AND g.schedule_id=@id
                    """, connection))
                {
                    command.Parameters.AddWithValue("org", tenant);
                    command.Parameters.AddWithValue("id", scheduleId);
                    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                        generated.Add(reader.GetInt32(0),
                            (reader.GetGuid(1), reader.GetString(2)));
                }

                var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
                var occurrences = new List<RecognitionOccurrenceResponse>(schedule.Months);
                for (var number = 1; number <= schedule.Months; number++)
                {
                    var date = schedule.FirstPostingDate.AddMonths(number - 1);
                    var amount = AmountFor(schedule, number);
                    var exists = generated.TryGetValue(number, out var posting);
                    var status = exists ? posting.Status :
                        date < today ? "Overdue" : date == today ? "Due" : "Scheduled";
                    occurrences.Add(new RecognitionOccurrenceResponse(
                        number, date, amount, status,
                        exists ? posting.Journal : null));
                }

                return new RecognitionScheduleDetailResponse(schedule, occurrences);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        });
    }

    public async Task<RecognitionScheduleResponse> CreateAsync(
        CreateRecognitionScheduleRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            throw new JournalConflictException("Recognition schedule body is required.");
        var tenant = organization.RequireOrganizationId();
        var actor = currentUser.RequireUserId();
        var kind = request.Kind?.Trim();
        var name = request.Name?.Trim();
        var description = request.Description?.Trim();

        if (kind is not ("PrepaidExpense" or "DeferredRevenue" or
            "AccruedExpense" or "AccruedRevenue") ||
            string.IsNullOrWhiteSpace(name) || name.Length is < 3 or > 140 ||
            string.IsNullOrWhiteSpace(description) || description.Length is < 3 or > 300 ||
            request.FirstPostingDate == default || request.FirstPostingDate.Day > 28 ||
            request.Months is < 1 or > 120 ||
            request.TotalAmount <= 0m || request.TotalAmount >= 1000000000000m ||
            decimal.Round(request.TotalAmount, 4) != request.TotalAmount ||
            request.DebitAccountId == Guid.Empty || request.CreditAccountId == Guid.Empty ||
            request.DebitAccountId == request.CreditAccountId)
            throw new JournalConflictException(
                "Valid recognition kind, 3-140 character name, description, posting day 1-28, 1-120 months, distinct accounts and positive four-decimal amount are required.");

        try
        {
            _ = request.FirstPostingDate.AddMonths(request.Months - 1);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new JournalConflictException("Recognition schedule exceeds the supported calendar.");
        }

        var monthly = decimal.Truncate(
            request.TotalAmount / request.Months * 10000m) / 10000m;
        if (monthly <= 0m)
            throw new JournalConflictException(
                "Total amount is too small for nonzero four-decimal monthly recognition.");

        var debitType = kind switch
        {
            "PrepaidExpense" or "AccruedExpense" => AccountType.Expense,
            "DeferredRevenue" => AccountType.Liability,
            _ => AccountType.Asset
        };
        var creditType = kind switch
        {
            "PrepaidExpense" => AccountType.Asset,
            "AccruedExpense" => AccountType.Liability,
            _ => AccountType.Revenue
        };
        var accountIds = new[] { request.DebitAccountId, request.CreditAccountId };
        var accounts = await db.Accounts.AsNoTracking()
            .Where(x => x.OrganizationId == tenant && accountIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Type, x.IsActive, x.IsAnalytical, x.AllowManualPosting })
            .ToArrayAsync(cancellationToken);
        if (accounts.Length != 2 ||
            accounts.Any(x => !x.IsActive || !x.IsAnalytical || !x.AllowManualPosting) ||
            accounts.SingleOrDefault(x => x.Id == request.DebitAccountId)?.Type != debitType ||
            accounts.SingleOrDefault(x => x.Id == request.CreditAccountId)?.Type != creditType)
            throw new JournalConflictException(
                "Recognition requires two tenant-owned active analytical posting accounts with correct balance sheet and profit/loss types.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var connection = (NpgsqlConnection)db.Database.GetDbConnection();
            var id = Guid.NewGuid();
            await using (var command = Cmd(connection, tx, """
                INSERT INTO accounting.recognition_schedules
                    (id,organization_id,kind,name,description,first_posting_date,
                     months,total_amount,monthly_amount,debit_account_id,
                     credit_account_id,created_by_user_id)
                VALUES (@id,@org,@kind,@name,@desc,@date,@months,
                        @total,@monthly,@debit,@credit,@actor)
                """, ("id", id), ("org", tenant), ("kind", kind!),
                ("name", name!), ("desc", description!),
                ("date", request.FirstPostingDate), ("months", request.Months),
                ("total", request.TotalAmount), ("monthly", monthly),
                ("debit", request.DebitAccountId),
                ("credit", request.CreditAccountId), ("actor", actor)))
                await command.ExecuteNonQueryAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return new RecognitionScheduleResponse(id, kind!, name!, description!,
                request.FirstPostingDate, request.Months, request.TotalAmount,
                monthly, request.DebitAccountId, request.CreditAccountId);
        });
    }

    public async Task<RecognitionGenerationResponse?> GenerateAsync(
        Guid scheduleId, GenerateRecognitionRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var actor = currentUser.RequireUserId();
        if (scheduleId == Guid.Empty || request is null || request.PeriodNumber < 1)
            throw new JournalConflictException(
                "A schedule and positive period number are required.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var connection = (NpgsqlConnection)db.Database.GetDbConnection();
            RecognitionScheduleResponse schedule;
            await using (var command = Cmd(connection, tx,
                SelectSchedule + " AND id=@id FOR UPDATE",
                ("org", tenant), ("id", scheduleId)))
            await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                schedule = ReadSchedule(reader);
            }

            if (request.PeriodNumber > schedule.Months)
                throw new JournalConflictException(
                    "Requested recognition period is outside the immutable schedule.");

            var date = schedule.FirstPostingDate.AddMonths(request.PeriodNumber - 1);
            var amount = AmountFor(schedule, request.PeriodNumber);
            var reference = $"recognition-schedule:v1:{scheduleId:N}:{request.PeriodNumber}";
            await using (var existing = Cmd(connection, tx, """
                SELECT g.journal_entry_id,e.status,e.external_reference,g.amount
                FROM accounting.recognition_generations g
                JOIN accounting.journal_entries e
                  ON e.id=g.journal_entry_id AND e.organization_id=g.organization_id
                WHERE g.organization_id=@org AND g.schedule_id=@schedule
                  AND g.period_number=@period
                """, ("org", tenant), ("schedule", scheduleId),
                ("period", request.PeriodNumber)))
            await using (var reader = await existing.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    if (reader.IsDBNull(2) || reader.GetString(2) != reference ||
                        reader.GetDecimal(3) != amount)
                        throw new JournalConflictException(
                            "Existing recognition generation does not match its immutable evidence.");
                    return new RecognitionGenerationResponse(
                        scheduleId, request.PeriodNumber, reader.GetGuid(0),
                        date, amount, reader.GetString(1), AlreadyGenerated: true);
                }
            }

            var initial = await calendars.FindPeriodByDateAsync(
                tenant, date, trackChanges: false, cancellationToken);
            if (initial is null)
                throw new JournalConflictException(
                    "No fiscal period contains the recognition posting date.");
            await journals.LockPostingCalendarAsync(
                tenant, initial.FiscalYearId, initial.Id, cancellationToken);
            var period = await calendars.GetPeriodAsync(
                tenant, initial.FiscalYearId, initial.Id,
                trackChanges: false, cancellationToken);
            var year = await calendars.GetFiscalYearAsync(
                tenant, initial.FiscalYearId, trackChanges: false, cancellationToken);
            if (period is null || year is null ||
                period.Status != AccountingPeriodStatus.Open ||
                year.Status != FiscalYearStatus.Open ||
                !period.Contains(date) || date < year.StartDate || date > year.EndDate)
                throw new JournalConflictException(
                    "Recognition requires an open fiscal year and accounting period.");

            // Revalidate account eligibility at generation, not only creation.
            var accounts = await db.Accounts.AsNoTracking()
                .Where(x => x.OrganizationId == tenant &&
                    (x.Id == schedule.DebitAccountId || x.Id == schedule.CreditAccountId))
                .Select(x => new { x.Id, x.Type, x.IsActive, x.IsAnalytical, x.AllowManualPosting })
                .ToArrayAsync(cancellationToken);
            var expectedDebit = schedule.Kind is "PrepaidExpense" or "AccruedExpense"
                ? AccountType.Expense
                : schedule.Kind == "DeferredRevenue" ? AccountType.Liability : AccountType.Asset;
            var expectedCredit = schedule.Kind switch
            {
                "PrepaidExpense" => AccountType.Asset,
                "AccruedExpense" => AccountType.Liability,
                _ => AccountType.Revenue
            };
            if (accounts.Length != 2 ||
                accounts.Any(x => !x.IsActive || !x.IsAnalytical || !x.AllowManualPosting) ||
                accounts.SingleOrDefault(x => x.Id == schedule.DebitAccountId)?.Type != expectedDebit ||
                accounts.SingleOrDefault(x => x.Id == schedule.CreditAccountId)?.Type != expectedCredit)
                throw new JournalConflictException(
                    "Recognition accounts were deactivated or changed their posting category.");

            if (await journals.GetSystemEntryByExternalReferenceAsync(
                tenant, reference, trackChanges: false, cancellationToken) is not null)
                throw new JournalConflictException(
                    "An unlinked recognition journal already exists for this period.");

            var now = clock.GetUtcNow();
            var draft = JournalEntry.CreateDraft(
                tenant, period.Id, date, date,
                $"{schedule.Kind}: {schedule.Name} — {schedule.Description}",
                JournalEntrySource.System, reference, now);
            await journals.AddEntryAsync(draft, cancellationToken);
            await journals.AddLineAsync(JournalLine.Create(
                tenant, draft.Id, 1, schedule.DebitAccountId,
                amount, 0m, $"Recognition period {request.PeriodNumber}", now),
                cancellationToken);
            await journals.AddLineAsync(JournalLine.Create(
                tenant, draft.Id, 2, schedule.CreditAccountId,
                0m, amount, $"Recognition period {request.PeriodNumber}", now),
                cancellationToken);
            await journals.SaveChangesAsync(cancellationToken);

            await using (var command = Cmd(connection, tx, """
                INSERT INTO accounting.recognition_generations
                    (id,organization_id,schedule_id,period_number,posting_date,
                     amount,journal_entry_id,generated_by_user_id)
                VALUES (@id,@org,@schedule,@number,@date,@amount,@journal,@actor)
                """, ("id", Guid.NewGuid()), ("org", tenant),
                ("schedule", scheduleId), ("number", request.PeriodNumber),
                ("date", date), ("amount", amount),
                ("journal", draft.Id), ("actor", actor)))
                await command.ExecuteNonQueryAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return new RecognitionGenerationResponse(
                scheduleId, request.PeriodNumber, draft.Id, date,
                amount, "Draft", AlreadyGenerated: false);
        });
    }

    private static decimal AmountFor(RecognitionScheduleResponse schedule, int number) =>
        number == schedule.Months
            ? schedule.TotalAmount - schedule.MonthlyAmount * (schedule.Months - 1)
            : schedule.MonthlyAmount;

    private static RecognitionScheduleResponse ReadSchedule(NpgsqlDataReader reader) =>
        new(reader.GetGuid(0), reader.GetString(1), reader.GetString(2),
            reader.GetString(3), reader.GetFieldValue<DateOnly>(4),
            reader.GetInt32(5), reader.GetDecimal(6), reader.GetDecimal(7),
            reader.GetGuid(8), reader.GetGuid(9));

    private static NpgsqlCommand Cmd(NpgsqlConnection connection,
        IDbContextTransaction transaction, string sql,
        params (string Name, object Value)[] values)
    {
        var command = new NpgsqlCommand(sql, connection,
            (NpgsqlTransaction)transaction.GetDbTransaction());
        foreach (var (name, value) in values)
            command.Parameters.AddWithValue(name, value);
        return command;
    }
}
