using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>
/// Append-only bank fee/realized FX correction evidence. Reversals are
/// ordinary immutable GL journal entries tied to the original source.
/// </summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261014000600_BankPostingCorrections")]
public sealed class BankPostingCorrections : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE banking.bank_posting_corrections (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                statement_line_id uuid NOT NULL,
                source_kind varchar(24) NOT NULL,
                original_journal_id uuid NOT NULL,
                reversal_journal_id uuid NOT NULL,
                correction_date date NOT NULL,
                reason varchar(500) NOT NULL,
                corrected_by_user_id uuid NOT NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT ck_bank_posting_correction_kind CHECK
                    (source_kind IN ('BankFee','ForeignSettlement')),
                CONSTRAINT ck_bank_posting_correction_reason CHECK
                    (length(trim(reason)) BETWEEN 10 AND 500),
                CONSTRAINT ck_bank_posting_correction_journal_diff CHECK
                    (original_journal_id <> reversal_journal_id),
                CONSTRAINT uq_bank_correction_source
                    UNIQUE (organization_id,statement_line_id,source_kind),
                CONSTRAINT uq_bank_correction_original
                    UNIQUE (organization_id,original_journal_id),
                CONSTRAINT uq_bank_correction_reversal
                    UNIQUE (organization_id,reversal_journal_id),
                CONSTRAINT fk_bank_correction_line
                    FOREIGN KEY(statement_line_id,organization_id)
                    REFERENCES banking.bank_statement_lines(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_bank_correction_original
                    FOREIGN KEY(original_journal_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_bank_correction_reversal
                    FOREIGN KEY(reversal_journal_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id)
                    ON DELETE RESTRICT
            );

            CREATE OR REPLACE FUNCTION banking.guard_bank_correction_immutable()
            RETURNS trigger LANGUAGE plpgsql AS $fn$
            BEGIN
                RAISE EXCEPTION 'Bank correction evidence is immutable';
            END $fn$;
            CREATE TRIGGER trg_bank_posting_correction_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_posting_corrections
                FOR EACH ROW EXECUTE FUNCTION banking.guard_bank_correction_immutable();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE banking.bank_posting_corrections;
            DROP FUNCTION banking.guard_bank_correction_immutable();
            """);
    }
}
