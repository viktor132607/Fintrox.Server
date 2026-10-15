using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fintrox.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FintroxDbContext))]
[Migration("20261016000200_RecurringJournalAutomation")]
public sealed class RecurringJournalAutomation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE accounting.recurring_journal_automation (
                organization_id uuid NOT NULL,
                template_id uuid NOT NULL,
                enabled boolean NOT NULL DEFAULT false,
                configured_by_user_id uuid NOT NULL,
                updated_at_utc timestamptz NOT NULL,
                PRIMARY KEY (organization_id,template_id),
                CONSTRAINT fk_rja_template FOREIGN KEY (template_id,organization_id)
                    REFERENCES accounting.recurring_journal_templates(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_rja_actor FOREIGN KEY (configured_by_user_id)
                    REFERENCES identity.users(id) ON DELETE RESTRICT
            );

            CREATE TABLE accounting.recurring_journal_automation_runs (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                template_id uuid NOT NULL,
                posting_date date NOT NULL,
                status varchar(16) NOT NULL DEFAULT 'Pending',
                attempt_count int NOT NULL DEFAULT 0,
                lease_token uuid NULL,
                lease_until_utc timestamptz NULL,
                next_attempt_at_utc timestamptz NOT NULL,
                journal_entry_id uuid NULL,
                last_error varchar(500) NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                updated_at_utc timestamptz NOT NULL DEFAULT now(),
                UNIQUE (organization_id,template_id,posting_date),
                CONSTRAINT fk_rja_run_config FOREIGN KEY (organization_id,template_id)
                    REFERENCES accounting.recurring_journal_automation(organization_id,template_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_rja_run_journal FOREIGN KEY (journal_entry_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT ck_rja_run_state CHECK
                    (status IN ('Pending','Claimed','Retry','Generated','Failed')),
                CONSTRAINT ck_rja_run_attempt CHECK (attempt_count BETWEEN 0 AND 5),
                CONSTRAINT ck_rja_lease CHECK (
                    (status='Claimed' AND lease_token IS NOT NULL AND lease_until_utc IS NOT NULL)
                    OR (status<>'Claimed' AND lease_token IS NULL AND lease_until_utc IS NULL)),
                CONSTRAINT ck_rja_generated CHECK (
                    (status='Generated' AND journal_entry_id IS NOT NULL)
                    OR (status<>'Generated' AND journal_entry_id IS NULL))
            );
            CREATE INDEX ix_rja_run_ready ON accounting.recurring_journal_automation_runs
                (next_attempt_at_utc,id)
                WHERE status IN ('Pending','Retry','Claimed');

            CREATE TABLE accounting.recurring_journal_automation_events (
                id uuid PRIMARY KEY,
                run_id uuid NOT NULL REFERENCES accounting.recurring_journal_automation_runs(id)
                    ON DELETE RESTRICT,
                attempt_number integer NOT NULL CHECK (attempt_number BETWEEN 1 AND 5),
                event_type varchar(16) NOT NULL CHECK
                    (event_type IN ('Claimed','Generated','Retry','Failed')),
                journal_entry_id uuid NULL,
                detail varchar(500) NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now()
            );
            CREATE INDEX ix_rja_events_run
                ON accounting.recurring_journal_automation_events(run_id,created_at_utc);
            CREATE TRIGGER trg_rja_events_immutable
                BEFORE UPDATE OR DELETE
                ON accounting.recurring_journal_automation_events
                FOR EACH ROW EXECUTE FUNCTION
                    accounting.guard_recurring_journal_evidence();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE accounting.recurring_journal_automation_events;
            DROP TABLE accounting.recurring_journal_automation_runs;
            DROP TABLE accounting.recurring_journal_automation;
            """);
    }
}
