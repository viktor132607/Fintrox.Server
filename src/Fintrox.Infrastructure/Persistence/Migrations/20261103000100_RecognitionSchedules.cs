using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>Append-only recognition plans and one-system-draft-per-period evidence.</summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261103000100_RecognitionSchedules")]
public sealed class RecognitionSchedules : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE accounting.recognition_schedules (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                kind varchar(24) NOT NULL,
                name varchar(140) NOT NULL,
                description varchar(300) NOT NULL,
                first_posting_date date NOT NULL,
                months integer NOT NULL,
                total_amount numeric(19,4) NOT NULL,
                monthly_amount numeric(19,4) NOT NULL,
                debit_account_id uuid NOT NULL,
                credit_account_id uuid NOT NULL,
                created_by_user_id uuid NOT NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_recognition_schedule_tenant UNIQUE(id,organization_id),
                CONSTRAINT ck_recognition_kind CHECK(kind IN
                    ('PrepaidExpense','DeferredRevenue','AccruedExpense','AccruedRevenue')),
                CONSTRAINT ck_recognition_months CHECK(months BETWEEN 1 AND 120),
                CONSTRAINT ck_recognition_posting_day CHECK(
                    EXTRACT(DAY FROM first_posting_date) BETWEEN 1 AND 28),
                CONSTRAINT ck_recognition_amount CHECK(
                    total_amount>0 AND monthly_amount>0 AND monthly_amount<=total_amount),
                CONSTRAINT ck_recognition_distinct_accounts CHECK(
                    debit_account_id<>credit_account_id),
                CONSTRAINT ck_recognition_name CHECK(length(trim(name)) BETWEEN 3 AND 140),
                CONSTRAINT ck_recognition_description CHECK(length(trim(description)) BETWEEN 3 AND 300),
                CONSTRAINT fk_recognition_organization FOREIGN KEY(organization_id)
                    REFERENCES core.organizations(id) ON DELETE RESTRICT,
                CONSTRAINT fk_recognition_debit FOREIGN KEY(debit_account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_recognition_credit FOREIGN KEY(credit_account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_recognition_actor FOREIGN KEY(created_by_user_id)
                    REFERENCES identity.users("Id") ON DELETE RESTRICT
            );
            CREATE INDEX ix_recognition_schedules_tenant
                ON accounting.recognition_schedules(organization_id,created_at_utc,id);

            CREATE TABLE accounting.recognition_generations (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                schedule_id uuid NOT NULL,
                period_number integer NOT NULL,
                posting_date date NOT NULL,
                amount numeric(19,4) NOT NULL CHECK(amount>0),
                journal_entry_id uuid NOT NULL,
                generated_by_user_id uuid NOT NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_recognition_generation_period UNIQUE(
                    organization_id,schedule_id,period_number),
                CONSTRAINT uq_recognition_generation_date UNIQUE(
                    organization_id,schedule_id,posting_date),
                CONSTRAINT uq_recognition_generation_journal UNIQUE(
                    organization_id,journal_entry_id),
                CONSTRAINT ck_recognition_generation_number CHECK(
                    period_number BETWEEN 1 AND 120),
                CONSTRAINT fk_recognition_generation_schedule
                    FOREIGN KEY(schedule_id,organization_id)
                    REFERENCES accounting.recognition_schedules(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_recognition_generation_journal
                    FOREIGN KEY(journal_entry_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_recognition_generation_actor
                    FOREIGN KEY(generated_by_user_id)
                    REFERENCES identity.users("Id") ON DELETE RESTRICT
            );
            CREATE TRIGGER trg_recognition_schedules_immutable
                BEFORE UPDATE OR DELETE ON accounting.recognition_schedules
                FOR EACH ROW EXECUTE FUNCTION accounting.guard_recurring_journal_evidence();
            CREATE TRIGGER trg_recognition_generations_immutable
                BEFORE UPDATE OR DELETE ON accounting.recognition_generations
                FOR EACH ROW EXECUTE FUNCTION accounting.guard_recurring_journal_evidence();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE accounting.recognition_generations;
            DROP TABLE accounting.recognition_schedules;
            """);
    }
}
