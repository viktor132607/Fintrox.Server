using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>Immutable one-to-one linkage between imported bank evidence and
/// the existing authoritative posted accounting journal.</summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261014000200_BankFeePosting")]
public sealed class BankFeePosting : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE banking.bank_fee_postings (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                statement_line_id uuid NOT NULL,
                journal_entry_id uuid NOT NULL,
                expense_account_id uuid NOT NULL,
                reason varchar(250) NOT NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_bank_fee_tenant_line UNIQUE(organization_id, statement_line_id),
                CONSTRAINT uq_bank_fee_tenant_journal UNIQUE(organization_id, journal_entry_id),
                CONSTRAINT fk_bank_fee_source FOREIGN KEY(statement_line_id, organization_id)
                    REFERENCES banking.bank_statement_lines(id, organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fee_journal FOREIGN KEY(journal_entry_id, organization_id)
                    REFERENCES accounting.journal_entries(id, organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fee_expense FOREIGN KEY(expense_account_id, organization_id)
                    REFERENCES accounting.accounts(id, organization_id) ON DELETE RESTRICT,
                CONSTRAINT ck_bank_fee_reason CHECK(length(trim(reason)) BETWEEN 10 AND 250)
            );
            CREATE INDEX ix_bank_fee_org_journal
                ON banking.bank_fee_postings(organization_id, journal_entry_id);

            -- An existing fee posting must never be removed or reassigned.
            CREATE OR REPLACE FUNCTION banking.guard_fee_posting_changes()
            RETURNS trigger LANGUAGE plpgsql AS $fn$
            BEGIN
                RAISE EXCEPTION 'Posted bank fee evidence is immutable';
            END $fn$;
            CREATE TRIGGER trg_bank_fee_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_fee_postings
                FOR EACH ROW EXECUTE FUNCTION banking.guard_fee_posting_changes();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE banking.bank_fee_postings;
            DROP FUNCTION banking.guard_fee_posting_changes();
            """);
    }
}
