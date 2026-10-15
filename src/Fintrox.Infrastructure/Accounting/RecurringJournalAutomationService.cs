using Fintrox.Application.Accounting;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// Opt-in UTC-day recurring draft scheduler. Claims use SKIP LOCKED with a
/// fenced lease token; generated GL journals keep their own unique source key.
/// Nothing in this service posts a journal.
/// </summary>
public sealed class RecurringJournalAutomationService(
    FintroxDbContext db,
    ICurrentOrganization organization,
    ICurrentUser currentUser,
    RecurringJournalService recurring,
    TimeProvider clock) : IRecurringJournalAutomationService
{
    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromHours(1)
    ];

    public async Task<RecurringJournalAutomationResponse?> ConfigureAsync(
        Guid templateId, ConfigureRecurringJournalAutomationRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var actor = currentUser.RequireUserId();
        if (templateId == Guid.Empty || request is null)
            throw new JournalConflictException("A template and enabled flag are required.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();

            // Serialize opt-in and version cutover with the existing generation
            // and revision transaction lock order (template, then configuration).
            await using (var template = Cmd(cn, tx, """
                SELECT 1 FROM accounting.recurring_journal_templates
                WHERE id=@template AND organization_id=@org FOR UPDATE
                """, ("template", templateId), ("org", tenant)))
            {
                if (await template.ExecuteScalarAsync(cancellationToken) is null)
                    return null;
            }

            if (request.Enabled)
            {
                await using var predecessor = Cmd(cn, tx, """
                    SELECT 1 FROM accounting.recurring_journal_template_revisions
                    WHERE organization_id=@org AND previous_template_id=@template
                    """, ("org", tenant), ("template", templateId));
                if (await predecessor.ExecuteScalarAsync(cancellationToken) is not null)
                    throw new JournalConflictException(
                        "Only the latest version of a recurring template may be enabled.");
            }

            var now = clock.GetUtcNow();
            await using (var upsert = Cmd(cn, tx, """
                INSERT INTO accounting.recurring_journal_automation
                    (organization_id,template_id,enabled,configured_by_user_id,updated_at_utc)
                VALUES (@org,@template,@enabled,@actor,@now)
                ON CONFLICT (organization_id,template_id)
                DO UPDATE SET enabled=EXCLUDED.enabled,
                              configured_by_user_id=EXCLUDED.configured_by_user_id,
                              updated_at_utc=EXCLUDED.updated_at_utc
                """, ("org", tenant), ("template", templateId),
                ("enabled", request.Enabled), ("actor", actor), ("now", now)))
                await upsert.ExecuteNonQueryAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return new RecurringJournalAutomationResponse(
                templateId, request.Enabled, actor, now);
        });
    }

    public async Task<RecurringJournalAutomationResponse?> GetConfigurationAsync(
        Guid templateId, CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var cn = (NpgsqlConnection)db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var cmd = new NpgsqlCommand("""
                SELECT enabled,configured_by_user_id,updated_at_utc
                FROM accounting.recurring_journal_automation
                WHERE organization_id=@org AND template_id=@template
                """, cn);
            cmd.Parameters.AddWithValue("org", tenant);
            cmd.Parameters.AddWithValue("template", templateId);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null;
            return new RecurringJournalAutomationResponse(
                templateId, reader.GetBoolean(0), reader.GetGuid(1),
                reader.GetFieldValue<DateTimeOffset>(2));
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    public async Task<IReadOnlyList<RecurringJournalAutomationRunResponse>?> GetRunsAsync(
        Guid templateId, CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var cn = (NpgsqlConnection)db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using (var exists = new NpgsqlCommand("""
                SELECT 1 FROM accounting.recurring_journal_templates
                WHERE organization_id=@org AND id=@template
                """, cn))
            {
                exists.Parameters.AddWithValue("org", tenant);
                exists.Parameters.AddWithValue("template", templateId);
                if (await exists.ExecuteScalarAsync(cancellationToken) is null)
                    return null;
            }

            var runs = new List<RecurringJournalAutomationRunResponse>();
            await using (var cmd = new NpgsqlCommand("""
                SELECT id,posting_date,status,attempt_count,journal_entry_id,
                       next_attempt_at_utc,last_error
                FROM accounting.recurring_journal_automation_runs
                WHERE organization_id=@org AND template_id=@template
                ORDER BY posting_date DESC LIMIT 200
                """, cn))
            {
                cmd.Parameters.AddWithValue("org", tenant);
                cmd.Parameters.AddWithValue("template", templateId);
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    runs.Add(new RecurringJournalAutomationRunResponse(
                        reader.GetGuid(0), templateId,
                        reader.GetFieldValue<DateOnly>(1), reader.GetString(2),
                        reader.GetInt32(3),
                        reader.IsDBNull(4) ? null : reader.GetGuid(4),
                        reader.GetFieldValue<DateTimeOffset>(5),
                        reader.IsDBNull(6) ? null : reader.GetString(6)));
            }
            return runs;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    public async Task<int> ProcessDueAsync(
        DateOnly utcDate, int maxItems, CancellationToken cancellationToken)
    {
        if (utcDate == default || maxItems is < 1 or > 20)
            throw new ArgumentOutOfRangeException(nameof(maxItems));

        await QueueTodaysOptedInTemplatesAsync(utcDate, cancellationToken);
        var completed = 0;
        for (var index = 0; index < maxItems; index++)
        {
            var claim = await ClaimNextAsync(utcDate, cancellationToken);
            if (claim is null) break;
            completed++;
            try
            {
                // Same idempotency source key as manual generation.
                // The worker has no HTTP tenant identity; GenerateForAutomation
                // checks the persisted explicit opt-in and current actor rights.
                var outcome = await recurring.GenerateForAutomationAsync(
                    claim.Value.Tenant, claim.Value.Actor, claim.Value.Template,
                    claim.Value.Date, cancellationToken);
                if (outcome is null)
                    throw new JournalConflictException(
                        "The opted-in recurring template was not found.");
                await FinalizeAsync(claim.Value, outcome.JournalEntryId,
                    error: null, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw; // An expired lease is retryable by another worker.
            }
            catch (Exception ex)
            {
                await FinalizeAsync(claim.Value, journal: null,
                    error: ex.Message, cancellationToken);
            }
        }
        return completed;
    }

    private async Task QueueTodaysOptedInTemplatesAsync(
        DateOnly date, CancellationToken cancellationToken)
    {
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                await using var cmd = new NpgsqlCommand("""
                    INSERT INTO accounting.recurring_journal_automation_runs
                        (id,organization_id,template_id,posting_date,status,
                         next_attempt_at_utc)
                    SELECT gen_random_uuid(),a.organization_id,a.template_id,@date,
                           'Pending',@now
                    FROM accounting.recurring_journal_automation a
                    JOIN accounting.recurring_journal_templates t
                      ON t.id=a.template_id AND t.organization_id=a.organization_id
                    JOIN core.organizations org ON org.id=a.organization_id
                    LEFT JOIN accounting.recurring_journal_template_revisions successor
                      ON successor.organization_id=a.organization_id
                     AND successor.previous_template_id=a.template_id
                    WHERE a.enabled AND org.is_active
                      AND t.day_of_month=EXTRACT(DAY FROM @date::date)
                      AND @date >= t.first_posting_date
                      AND (t.last_posting_date IS NULL OR @date <= t.last_posting_date)
                      AND (successor.effective_from_month IS NULL
                           OR @date < successor.effective_from_month)
                    ON CONFLICT (organization_id,template_id,posting_date) DO NOTHING
                    """, cn);
                cmd.Parameters.AddWithValue("date", date);
                cmd.Parameters.AddWithValue("now", clock.GetUtcNow());
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        });
    }

    private async Task<AutomationClaim?> ClaimNextAsync(
        DateOnly utcDate, CancellationToken cancellationToken)
    {
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            var now = clock.GetUtcNow();
            var token = Guid.NewGuid();

            // A crashed claim becomes retryable after the lease.
            // Out-of-budget crashed claims become permanently Failed.
            await using (var exhausted = Cmd(cn, tx, """
                UPDATE accounting.recurring_journal_automation_runs
                SET status='Failed',lease_token=NULL,lease_until_utc=NULL,
                    last_error='Lease expired after maximum attempts',
                    updated_at_utc=@now
                WHERE status='Claimed' AND attempt_count >= 5
                  AND lease_until_utc <= @now
                """, ("now", now)))
                await exhausted.ExecuteNonQueryAsync(cancellationToken);

            AutomationClaim? result = null;
            await using (var cmd = Cmd(cn, tx, """
                WITH due AS (
                  SELECT r.id
                  FROM accounting.recurring_journal_automation_runs r
                  JOIN accounting.recurring_journal_automation a
                    ON a.organization_id=r.organization_id
                   AND a.template_id=r.template_id
                  WHERE a.enabled AND r.attempt_count < 5
                    AND r.posting_date >= (@today::date - INTERVAL '7 days')::date
                    AND ((r.status IN ('Pending','Retry')
                          AND r.next_attempt_at_utc <= @now)
                      OR (r.status='Claimed' AND r.lease_until_utc <= @now))
                  ORDER BY r.next_attempt_at_utc,r.id
                  FOR UPDATE OF r SKIP LOCKED
                  LIMIT 1
                )
                UPDATE accounting.recurring_journal_automation_runs r
                SET status='Claimed',attempt_count=r.attempt_count+1,
                    lease_token=@token,lease_until_utc=@until,
                    updated_at_utc=@now
                FROM due,accounting.recurring_journal_automation a
                WHERE r.id=due.id AND a.organization_id=r.organization_id
                  AND a.template_id=r.template_id
                RETURNING r.id,r.organization_id,r.template_id,r.posting_date,
                          r.attempt_count,a.configured_by_user_id
                """, ("today", utcDate), ("now", now), ("token", token),
                ("until", now.Add(Lease))))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                    result = new AutomationClaim(reader.GetGuid(0),
                        reader.GetGuid(1), reader.GetGuid(2),
                        reader.GetFieldValue<DateOnly>(3),
                        reader.GetInt32(4), reader.GetGuid(5), token);
            }

            if (result is { } claimed)
            {
                await using var log = Cmd(cn, tx, """
                    INSERT INTO accounting.recurring_journal_automation_events
                        (id,run_id,attempt_number,event_type)
                    VALUES (@id,@run,@attempt,'Claimed')
                    """, ("id", Guid.NewGuid()), ("run", claimed.Id),
                    ("attempt", claimed.Attempt));
                await log.ExecuteNonQueryAsync(cancellationToken);
            }
            await tx.CommitAsync(cancellationToken);
            return result;
        });
    }

    private async Task FinalizeAsync(
        AutomationClaim claim, Guid? journal, string? error,
        CancellationToken cancellationToken)
    {
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            var now = clock.GetUtcNow();
            var state = journal.HasValue ? "Generated" :
                claim.Attempt >= 5 ? "Failed" : "Retry";
            var delay = RetryDelays[Math.Clamp(claim.Attempt - 1, 0, RetryDelays.Length - 1)];
            var message = error is null ? null :
                error.Length > 500 ? error[..500] : error;

            int written;
            await using (var update = Cmd(cn, tx, """
                UPDATE accounting.recurring_journal_automation_runs
                SET status=@status,lease_token=NULL,lease_until_utc=NULL,
                    next_attempt_at_utc=@next,journal_entry_id=@journal,
                    last_error=@error,updated_at_utc=@now
                WHERE id=@id AND organization_id=@org
                  AND status='Claimed' AND lease_token=@token
                """, ("status", state), ("next", now.Add(delay)),
                ("journal", (object?)journal ?? DBNull.Value),
                ("error", (object?)message ?? DBNull.Value),
                ("now", now), ("id", claim.Id), ("org", claim.Tenant),
                ("token", claim.Token)))
                written = await update.ExecuteNonQueryAsync(cancellationToken);

            // CAS fencing: stale worker must not overwrite a newer lease
            // or add an outcome event. The GL source key still prevents dupes.
            if (written == 1)
            {
                await using var log = Cmd(cn, tx, """
                    INSERT INTO accounting.recurring_journal_automation_events
                        (id,run_id,attempt_number,event_type,
                         journal_entry_id,detail)
                    VALUES (@id,@run,@attempt,@event,@journal,@detail)
                    """, ("id", Guid.NewGuid()), ("run", claim.Id),
                    ("attempt", claim.Attempt), ("event", state),
                    ("journal", (object?)journal ?? DBNull.Value),
                    ("detail", (object?)message ?? DBNull.Value));
                await log.ExecuteNonQueryAsync(cancellationToken);
            }
            await tx.CommitAsync(cancellationToken);
        });
    }

    private static NpgsqlCommand Cmd(
        NpgsqlConnection cn, IDbContextTransaction tx, string sql,
        params (string Name, object Value)[] args)
    {
        var cmd = new NpgsqlCommand(
            sql, cn, (NpgsqlTransaction)tx.GetDbTransaction());
        foreach (var (name, value) in args)
            cmd.Parameters.AddWithValue(name, value);
        return cmd;
    }

    private readonly record struct AutomationClaim(
        Guid Id, Guid Tenant, Guid Template, DateOnly Date,
        int Attempt, Guid Actor, Guid Token);
}
