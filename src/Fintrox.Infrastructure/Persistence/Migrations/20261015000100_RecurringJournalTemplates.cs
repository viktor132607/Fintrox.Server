using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Fintrox.Infrastructure.Persistence;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>
/// Immutable monthly manual-run recurring journal templates and generation ledger.
/// Generated entries are Draft until an authorized GL posting action.
/// </summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261015000100_RecurringJournalTemplates")]
public sealed class RecurringJournalTemplates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE accounting.recurring_journal_templates (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                name varchar(140) NOT NULL,
                description varchar(500) NOT NULL,
                first_posting_date date NOT NULL,
                last_posting_date date NULL,
                day_of_month smallint NOT NULL,
                created_by_user_id uuid NOT NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_recurring_template_tenant_id UNIQUE (id,organization_id),
                CONSTRAINT ck_recurring_template_day CHECK (day_of_month BETWEEN 1 AND 28),
                CONSTRAINT ck_recurring_template_first_day
                    CHECK (EXTRACT(DAY FROM first_posting_date) = day_of_month),
                CONSTRAINT ck_recurring_template_last
                    CHECK(last_posting_date IS NULL OR last_posting_date >= first_posting_date),
                CONSTRAINT ck_recurring_template_name CHECK(length(trim(name)) BETWEEN 3 AND 140),
                CONSTRAINT ck_recurring_template_description CHECK(length(trim(description)) BETWEEN 3 AND 500),
                CONSTRAINT fk_recurring_template_organization FOREIGN KEY(organization_id)
                    REFERENCES core.organizations(id) ON DELETE RESTRICT
            );

            CREATE TABLE accounting.recurring_journal_template_lines (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                template_id uuid NOT NULL,
                line_number integer NOT NULL CHECK(line_number BETWEEN 1 AND 100),
                account_id uuid NOT NULL,
                debit numeric(19,4) NOT NULL DEFAULT 0,
                credit numeric(19,4) NOT NULL DEFAULT 0,
                description varchar(500) NULL,
                CONSTRAINT uq_recurring_template_line UNIQUE(organization_id,template_id,line_number),
                CONSTRAINT ck_recurring_template_line_amount CHECK(
                    (debit > 0 AND credit = 0) OR
                    (credit > 0 AND debit = 0)),
                CONSTRAINT fk_recurring_template_line_template
                    FOREIGN KEY(template_id,organization_id)
                    REFERENCES accounting.recurring_journal_templates(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_recurring_template_line_account
                    FOREIGN KEY(account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id)
                    ON DELETE RESTRICT
            );

            CREATE TABLE accounting.recurring_journal_generations (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                template_id uuid NOT NULL,
                posting_date date NOT NULL,
                journal_entry_id uuid NOT NULL,
                generated_by_user_id uuid NOT NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_recurring_generation_period UNIQUE(organization_id,template_id,posting_date),
                CONSTRAINT uq_recurring_generation_journal UNIQUE(organization_id,journal_entry_id),
                CONSTRAINT fk_recurring_generation_template
                    FOREIGN KEY(template_id,organization_id)
                    REFERENCES accounting.recurring_journal_templates(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_recurring_generation_journal
                    FOREIGN KEY(journal_entry_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id)
                    ON DELETE RESTRICT
            );

            CREATE OR REPLACE FUNCTION accounting.guard_recurring_journal_evidence()
            RETURNS trigger LANGUAGE plpgsql AS $fn$
            BEGIN
                RAISE EXCEPTION 'Recurring journal templates and generated evidence are immutable; create a new template';
            END $fn$;
            CREATE TRIGGER trg_recurring_template_immutable
                BEFORE UPDATE OR DELETE ON accounting.recurring_journal_templates
                FOR EACH ROW EXECUTE FUNCTION accounting.guard_recurring_journal_evidence();
            CREATE TRIGGER trg_recurring_template_line_immutable
                BEFORE UPDATE OR DELETE ON accounting.recurring_journal_template_lines
                FOR EACH ROW EXECUTE FUNCTION accounting.guard_recurring_journal_evidence();
            CREATE TRIGGER trg_recurring_generation_immutable
                BEFORE UPDATE OR DELETE ON accounting.recurring_journal_generations
                FOR EACH ROW EXECUTE FUNCTION accounting.guard_recurring_journal_evidence();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE accounting.recurring_journal_generations;
            DROP TABLE accounting.recurring_journal_template_lines;
            DROP TABLE accounting.recurring_journal_templates;
            DROP FUNCTION accounting.guard_recurring_journal_evidence();
            """);
    }
}
