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
/// Immutable monthly recurring templates. Explicit generation creates one
/// System Draft in an open period. GL posting remains a separate authorized
/// action, with a journal number allocated only on actual posting.
/// </summary>
public sealed class RecurringJournalService(
    FintroxDbContext db,
    ICurrentOrganization organization,
    ICurrentUser currentUser,
    IJournalRepository journals,
    IFiscalCalendarRepository calendars,
    TimeProvider clock) : IRecurringJournalService
{
    public async Task<IReadOnlyList<RecurringJournalTemplateResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                var templates = new List<(Guid Id, string Name, string Description,
                    DateOnly First, DateOnly? Last, int Day)>();
                await using (var cmd = new NpgsqlCommand("""
                    SELECT id,name,description,first_posting_date,last_posting_date,day_of_month
                    FROM accounting.recurring_journal_templates
                    WHERE organization_id=@org ORDER BY created_at_utc,id
                    """, cn))
                {
                    cmd.Parameters.AddWithValue("org", tenant);
                    await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        templates.Add((reader.GetGuid(0), reader.GetString(1),
                            reader.GetString(2), reader.GetFieldValue<DateOnly>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<DateOnly>(4),
                            reader.GetInt16(5)));
                    }
                }

                var results = new List<RecurringJournalTemplateResponse>();
                foreach (var t in templates)
                {
                    var lines = new List<CreateJournalLineRequest>();
                    await using var cmd = new NpgsqlCommand("""
                        SELECT account_id,debit,credit,description
                        FROM accounting.recurring_journal_template_lines
                        WHERE organization_id=@org AND template_id=@template
                        ORDER BY line_number
                        """, cn);
                    cmd.Parameters.AddWithValue("org", tenant);
                    cmd.Parameters.AddWithValue("template", t.Id);
                    await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                        lines.Add(new CreateJournalLineRequest(
                            reader.GetGuid(0), reader.GetDecimal(1),
                            reader.GetDecimal(2), reader.IsDBNull(3) ? null : reader.GetString(3)));
                    results.Add(new RecurringJournalTemplateResponse(
                        t.Id, t.Name, t.Description, t.First, t.Last, t.Day, lines));
                }
                return (IReadOnlyList<RecurringJournalTemplateResponse>)results;
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        });
    }

    public async Task<RecurringJournalTemplateResponse> CreateAsync(
        CreateRecurringJournalTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var actor = currentUser.RequireUserId();
        var name = request.Name?.Trim();
        var description = request.Description?.Trim();
        var first = request.FirstPostingDate;
        var last = request.LastPostingDate;
        var lines = request.Lines;
        if (string.IsNullOrWhiteSpace(name) || name.Length is < 3 or > 140 ||
            string.IsNullOrWhiteSpace(description) || description.Length is < 3 or > 500 ||
            first == default || first.Day is < 1 or > 28 ||
            last.HasValue && last.Value < first ||
            lines is null || lines.Count is < 2 or > 100)
            throw new JournalConflictException(
                "Recurring template needs a 3-140 character name, description, day 1-28, valid end date and 2-100 lines.");

        decimal debit = 0m, credit = 0m;
        foreach (var line in lines!)
        {
            if (line.AccountId == Guid.Empty || line.Debit < 0m || line.Credit < 0m ||
                (line.Debit == 0m && line.Credit == 0m) ||
                (line.Debit != 0m && line.Credit != 0m) ||
                decimal.Round(line.Debit, 4) != line.Debit ||
                decimal.Round(line.Credit, 4) != line.Credit ||
                line.Description?.Length > 500)
                throw new JournalConflictException(
                    "Each recurring journal line must contain one positive four-decimal debit or credit and an active account.");
            debit += line.Debit;
            credit += line.Credit;
        }
        if (debit <= 0m || debit != credit)
            throw new JournalConflictException("Recurring journal lines must balance.");

        // Validate all account ownership and current posting eligibility
        // before persisting the immutable template.
        var accountIds = lines.Select(x => x.AccountId).Distinct().ToArray();
        var accounts = await db.Accounts.AsNoTracking()
            .Where(x => x.OrganizationId == tenant && accountIds.Contains(x.Id))
            .Select(x => new { x.Id, x.IsActive, x.IsAnalytical, x.AllowManualPosting })
            .ToArrayAsync(cancellationToken);
        if (accounts.Length != accountIds.Length ||
            accounts.Any(x => !x.IsActive || !x.IsAnalytical || !x.AllowManualPosting))
            throw new JournalConflictException(
                "All recurring template accounts must be active tenant-owned analytical GL accounts permitting manual posting.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            var id = Guid.NewGuid();
            await using (var cmd = Command(cn, tx, """
                INSERT INTO accounting.recurring_journal_templates
                    (id,organization_id,name,description,first_posting_date,
                     last_posting_date,day_of_month,created_by_user_id)
                VALUES (@id,@org,@name,@desc,@first,@last,@day,@actor)
                """, ("id", id), ("org", tenant), ("name", name!),
                ("desc", description!), ("first", first),
                ("last", (object?)last ?? DBNull.Value),
                ("day", (short)first.Day), ("actor", actor)))
                await cmd.ExecuteNonQueryAsync(cancellationToken);

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                await using var cmd = Command(cn, tx, """
                    INSERT INTO accounting.recurring_journal_template_lines
                        (id,organization_id,template_id,line_number,
                         account_id,debit,credit,description)
                    VALUES (@id,@org,@template,@number,@account,@debit,@credit,@desc)
                    """, ("id", Guid.NewGuid()), ("org", tenant),
                    ("template", id), ("number", i + 1), ("account", line.AccountId),
                    ("debit", line.Debit), ("credit", line.Credit),
                    ("desc", (object?)line.Description ?? DBNull.Value));
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            await tx.CommitAsync(cancellationToken);
            return new RecurringJournalTemplateResponse(
                id, name!, description!, first, last, first.Day,
                lines.ToArray());
        });
    }

    public async Task<RecurringJournalGenerationResponse?> GenerateAsync(
        Guid templateId, GenerateRecurringJournalRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var actor = currentUser.RequireUserId();
        if (templateId == Guid.Empty || request.PostingDate == default)
            throw new JournalConflictException("Template and posting date are mandatory.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            string name, description;
            DateOnly first;
            DateOnly? last;
            short day;
            await using (var cmd = Command(cn, tx, """
                SELECT name,description,first_posting_date,last_posting_date,day_of_month
                FROM accounting.recurring_journal_templates
                WHERE id=@id AND organization_id=@org FOR UPDATE
                """, ("id", templateId), ("org", tenant)))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                name = reader.GetString(0);
                description = reader.GetString(1);
                first = reader.GetFieldValue<DateOnly>(2);
                last = reader.IsDBNull(3) ? null : reader.GetFieldValue<DateOnly>(3);
                day = reader.GetInt16(4);
            }

            var date = request.PostingDate;
            if (date < first || (last.HasValue && date > last.Value) ||
                date.Day != day)
                throw new JournalConflictException(
                    "Requested date does not match the immutable monthly recurrence schedule.");

            // Idempotency row is locked by the template FOR UPDATE lock.
            // Already-generated entries remain reviewable even after closure.
            await using (var existing = Command(cn, tx, """
                SELECT g.journal_entry_id,e.status,e.external_reference
                FROM accounting.recurring_journal_generations g
                JOIN accounting.journal_entries e
                  ON e.id=g.journal_entry_id AND e.organization_id=g.organization_id
                WHERE g.organization_id=@org AND g.template_id=@id AND g.posting_date=@date
                """, ("org", tenant), ("id", templateId), ("date", date)))
            await using (var reader = await existing.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    var journal = reader.GetGuid(0);
                    var expected = $"recurring-journal:v1:{templateId:N}:{date:yyyyMMdd}";
                    if (reader.IsDBNull(2) || reader.GetString(2) != expected)
                        throw new JournalConflictException(
                            "The generated journal has lost its immutable template reference.");
                    return new RecurringJournalGenerationResponse(
                        templateId, journal, date, reader.GetString(1),
                        AlreadyGenerated: true);
                }
            }

            var initial = await calendars.FindPeriodByDateAsync(
                tenant, date, trackChanges: false, cancellationToken);
            if (initial is null)
                throw new JournalConflictException(
                    "No accounting period contains the recurring journal date.");

            await journals.LockPostingCalendarAsync(
                tenant, initial.FiscalYearId, initial.Id, cancellationToken);
            var period = await calendars.GetPeriodAsync(
                tenant, initial.FiscalYearId, initial.Id,
                trackChanges: false, cancellationToken);
            var year = await calendars.GetFiscalYearAsync(
                tenant, initial.FiscalYearId, trackChanges: false, cancellationToken);
            if (year is null || period is null ||
                year.Status != FiscalYearStatus.Open ||
                period.Status != AccountingPeriodStatus.Open ||
                !period.Contains(date) || date < year.StartDate || date > year.EndDate)
                throw new JournalConflictException(
                    "Recurring journal can be generated only in an open accounting period and fiscal year.");

            var lines = new List<(int Number, Guid Account, decimal Debit, decimal Credit, string? Description)>();
            await using (var cmd = Command(cn, tx, """
                SELECT line_number,account_id,debit,credit,description
                FROM accounting.recurring_journal_template_lines
                WHERE organization_id=@org AND template_id=@id
                ORDER BY line_number
                """, ("org", tenant), ("id", templateId)))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    lines.Add((reader.GetInt32(0), reader.GetGuid(1),
                        reader.GetDecimal(2), reader.GetDecimal(3),
                        reader.IsDBNull(4) ? null : reader.GetString(4)));
            }
            if (lines.Count < 2 ||
                lines.Sum(x => x.Debit) != lines.Sum(x => x.Credit))
                throw new JournalConflictException("Recurring template is not balanced.");

            var accountIds = lines.Select(x => x.Account).Distinct().ToArray();
            var activeCount = await db.Accounts.AsNoTracking().CountAsync(
                x => x.OrganizationId == tenant &&
                    accountIds.Contains(x.Id) && x.IsActive &&
                    x.IsAnalytical && x.AllowManualPosting, cancellationToken);
            if (activeCount != accountIds.Length)
                throw new JournalConflictException(
                    "A recurring template account was deactivated or no longer allows posting.");

            var reference = $"recurring-journal:v1:{templateId:N}:{date:yyyyMMdd}";
            if (await journals.GetSystemEntryByExternalReferenceAsync(
                tenant, reference, trackChanges: false, cancellationToken) is not null)
                throw new JournalConflictException(
                    "An unlinked recurring journal already exists for this date.");

            var now = clock.GetUtcNow();
            var draft = JournalEntry.CreateDraft(
                tenant, period.Id, date, date,
                $"Recurring: {name} — {description}",
                JournalEntrySource.System, reference, now);
            await journals.AddEntryAsync(draft, cancellationToken);
            foreach (var line in lines)
                await journals.AddLineAsync(JournalLine.Create(
                    tenant, draft.Id, line.Number, line.Account, line.Debit,
                    line.Credit, line.Description, now), cancellationToken);
            await journals.SaveChangesAsync(cancellationToken);

            await using (var save = Command(cn, tx, """
                INSERT INTO accounting.recurring_journal_generations
                    (id,organization_id,template_id,posting_date,
                     journal_entry_id,generated_by_user_id)
                VALUES (@id,@org,@template,@date,@journal,@actor)
                """, ("id", Guid.NewGuid()), ("org", tenant),
                ("template", templateId), ("date", date),
                ("journal", draft.Id), ("actor", actor)))
                await save.ExecuteNonQueryAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return new RecurringJournalGenerationResponse(
                templateId, draft.Id, date, "Draft",
                AlreadyGenerated: false);
        });
    }


    /// <summary>
    /// Read-only tenant schedule projection; never generates or posts entries.
    /// One PostgreSQL statement joins immutable generation and GL evidence.
    /// </summary>
    public async Task<RecurringJournalScheduleResponse?> GetScheduleAsync(
        Guid templateId, DateOnly fromDate, DateOnly toDate,
        DateOnly? asOfDate, CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        if (templateId == Guid.Empty || fromDate == default || toDate == default ||
            toDate < fromDate || toDate.DayNumber - fromDate.DayNumber >= 366 ||
            asOfDate == DateOnly.MinValue)
            throw new JournalConflictException(
                "A valid template and inclusive date window of at most 366 days are required.");

        var today = asOfDate ?? DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                DateOnly first = default;
                DateOnly? last = null;
                short day = 0;
                var found = false;
                var generated = new Dictionary<DateOnly, (Guid JournalId, string Status)>();
                await using (var cmd = new NpgsqlCommand("""
                    SELECT t.first_posting_date,t.last_posting_date,t.day_of_month,
                           g.posting_date,g.journal_entry_id,e.status,e.external_reference
                    FROM accounting.recurring_journal_templates t
                    LEFT JOIN accounting.recurring_journal_generations g
                      ON g.organization_id=t.organization_id AND g.template_id=t.id
                     AND g.posting_date BETWEEN @from AND @to
                    LEFT JOIN accounting.journal_entries e
                      ON e.organization_id=g.organization_id AND e.id=g.journal_entry_id
                    WHERE t.organization_id=@org AND t.id=@template
                    ORDER BY g.posting_date
                    """, cn))
                {
                    cmd.Parameters.AddWithValue("org", tenant);
                    cmd.Parameters.AddWithValue("template", templateId);
                    cmd.Parameters.AddWithValue("from", fromDate);
                    cmd.Parameters.AddWithValue("to", toDate);
                    await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        found = true;
                        first = reader.GetFieldValue<DateOnly>(0);
                        last = reader.IsDBNull(1) ? null : reader.GetFieldValue<DateOnly>(1);
                        day = reader.GetInt16(2);
                        if (reader.IsDBNull(3)) continue;

                        var date = reader.GetFieldValue<DateOnly>(3);
                        if (reader.IsDBNull(4) || reader.IsDBNull(5) || reader.IsDBNull(6) ||
                            reader.GetString(6) !=
                                $"recurring-journal:v1:{templateId:N}:{date:yyyyMMdd}")
                            throw new JournalConflictException(
                                "Recurring journal generation evidence does not match its GL source.");
                        generated.Add(date, (reader.GetGuid(4), reader.GetString(5)));
                    }
                }

                if (!found) return null;
                var occurrences = new List<RecurringJournalOccurrenceResponse>();
                var month = new DateOnly(fromDate.Year, fromDate.Month, 1);
                while (month <= toDate)
                {
                    var date = new DateOnly(month.Year, month.Month, day);
                    if (date >= fromDate && date <= toDate &&
                        date >= first && (!last.HasValue || date <= last.Value))
                    {
                        if (generated.TryGetValue(date, out var evidence))
                        {
                            if (evidence.Status is not ("Draft" or "Posted" or "Reversed"))
                                throw new JournalConflictException(
                                    "Unexpected status for a generated recurring GL journal.");
                            occurrences.Add(new RecurringJournalOccurrenceResponse(
                                templateId, date, evidence.Status,
                                evidence.JournalId, evidence.Status));
                        }
                        else
                        {
                            var status = date < today ? "Overdue" :
                                date == today ? "Due" : "Scheduled";
                            occurrences.Add(new RecurringJournalOccurrenceResponse(
                                templateId, date, status, null, null));
                        }
                    }
                    if (month.Year == 9999 && month.Month == 12) break;
                    month = month.AddMonths(1);
                }

                return new RecurringJournalScheduleResponse(
                    templateId, fromDate, toDate, today, occurrences);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        });
    }

    private static NpgsqlCommand Command(
        NpgsqlConnection connection, IDbContextTransaction tx,
        string sql, params (string Name, object Value)[] args)
    {
        var cmd = new NpgsqlCommand(
            sql, connection, (NpgsqlTransaction)tx.GetDbTransaction());
        foreach (var (name, value) in args)
            cmd.Parameters.AddWithValue(name, value);
        return cmd;
    }
}
