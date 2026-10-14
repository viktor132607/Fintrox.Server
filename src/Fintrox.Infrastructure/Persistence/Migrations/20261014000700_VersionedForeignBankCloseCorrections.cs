using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>
/// Append-only replacement valuations and independent new review chain.
/// Never edits original revaluation or previously signed internal evidence.
/// </summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261014000700_VersionedForeignBankCloseCorrections")]
public sealed class VersionedForeignBankCloseCorrections : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS ux_bank_fx_original_tenant
                ON banking.bank_fx_revaluations(id,organization_id);

            CREATE TABLE banking.bank_fx_revaluation_amendments (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                original_revaluation_id uuid NOT NULL,
                bank_account_id uuid NOT NULL,
                statement_id uuid NOT NULL,
                cutoff_date date NOT NULL,
                version_number integer NOT NULL CHECK(version_number>=2),
                previous_journal_id uuid NULL,
                reversal_journal_id uuid NULL,
                replacement_journal_id uuid NULL,
                exchange_rate_id uuid NOT NULL,
                approved_rate numeric(22,10) NOT NULL CHECK(approved_rate>0),
                native_closing_balance numeric(19,4) NOT NULL CHECK(native_closing_balance>=0),
                ledger_base_before numeric(19,4) NOT NULL,
                previous_target_base numeric(19,4) NOT NULL,
                previous_adjustment_base numeric(19,4) NOT NULL,
                replacement_target_base numeric(19,4) NOT NULL,
                replacement_adjustment_base numeric(19,4) NOT NULL,
                gain_account_id uuid NOT NULL,
                loss_account_id uuid NOT NULL,
                amended_by_user_id uuid NOT NULL,
                reason varchar(400) NOT NULL,
                amended_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_fx_amendment_version
                    UNIQUE(organization_id,statement_id,version_number),
                CONSTRAINT uq_fx_amendment_previous_journal
                    UNIQUE(organization_id,previous_journal_id),
                CONSTRAINT uq_fx_amendment_reversal
                    UNIQUE(organization_id,reversal_journal_id),
                CONSTRAINT uq_fx_amendment_replacement
                    UNIQUE(organization_id,replacement_journal_id),
                CONSTRAINT uq_fx_amendment_tenant_id
                    UNIQUE(id,organization_id),
                CONSTRAINT fk_fx_amendment_original
                    FOREIGN KEY(original_revaluation_id,organization_id)
                    REFERENCES banking.bank_fx_revaluations(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_fx_amendment_account
                    FOREIGN KEY(bank_account_id,organization_id)
                    REFERENCES banking.bank_accounts(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_fx_amendment_statement
                    FOREIGN KEY(statement_id,organization_id)
                    REFERENCES banking.bank_statements(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_fx_amendment_rate
                    FOREIGN KEY(exchange_rate_id,organization_id)
                    REFERENCES accounting.exchange_rates(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_fx_amendment_gain
                    FOREIGN KEY(gain_account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_fx_amendment_loss
                    FOREIGN KEY(loss_account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_fx_amendment_previous_journal
                    FOREIGN KEY(previous_journal_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_fx_amendment_reversal_journal
                    FOREIGN KEY(reversal_journal_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_fx_amendment_replacement_journal
                    FOREIGN KEY(replacement_journal_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT ck_fx_amendment_reason
                    CHECK(length(trim(reason)) BETWEEN 10 AND 400),
                CONSTRAINT ck_fx_amendment_journals
                    CHECK((previous_adjustment_base=0 AND
                           previous_journal_id IS NULL AND reversal_journal_id IS NULL)
                          OR (previous_adjustment_base<>0 AND
                              previous_journal_id IS NOT NULL AND reversal_journal_id IS NOT NULL)),
                CONSTRAINT ck_fx_amendment_replacement
                    CHECK((replacement_adjustment_base=0 AND replacement_journal_id IS NULL)
                          OR (replacement_adjustment_base<>0 AND replacement_journal_id IS NOT NULL)),
                CONSTRAINT ck_fx_amendment_arithmetic
                    CHECK(previous_target_base=ledger_base_before+previous_adjustment_base
                      AND replacement_target_base=ledger_base_before+replacement_adjustment_base)
            );

            CREATE TABLE banking.bank_fx_amendment_preparations (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                amendment_id uuid NOT NULL,
                prepared_by_user_id uuid NOT NULL,
                evidence_sha256 char(64) NOT NULL,
                review_note varchar(500) NOT NULL,
                prepared_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_fx_amendment_preparation
                    UNIQUE(organization_id,amendment_id),
                CONSTRAINT uq_fx_amendment_prep_tenant_id UNIQUE(id,organization_id),
                CONSTRAINT fk_fx_amendment_preparation
                    FOREIGN KEY(amendment_id,organization_id)
                    REFERENCES banking.bank_fx_revaluation_amendments(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT ck_fx_amendment_prep_note
                    CHECK(length(trim(review_note)) BETWEEN 10 AND 500)
            );
            CREATE TABLE banking.bank_fx_amendment_approvals (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                preparation_id uuid NOT NULL,
                approved_by_user_id uuid NOT NULL,
                evidence_sha256 char(64) NOT NULL,
                approval_note varchar(500) NOT NULL,
                approved_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_fx_amendment_approval
                    UNIQUE(organization_id,preparation_id),
                CONSTRAINT fk_fx_amendment_approval
                    FOREIGN KEY(preparation_id,organization_id)
                    REFERENCES banking.bank_fx_amendment_preparations(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT ck_fx_amendment_approval_note
                    CHECK(length(trim(approval_note)) BETWEEN 10 AND 500)
            );

            CREATE OR REPLACE FUNCTION banking.guard_fx_amendment_immutable()
            RETURNS trigger LANGUAGE plpgsql AS $body$
            BEGIN
                RAISE EXCEPTION 'Versioned bank FX correction and review history is immutable';
            END $body$;
            CREATE TRIGGER trg_fx_amendment_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_fx_revaluation_amendments
                FOR EACH ROW EXECUTE FUNCTION banking.guard_fx_amendment_immutable();
            CREATE TRIGGER trg_fx_amendment_prep_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_fx_amendment_preparations
                FOR EACH ROW EXECUTE FUNCTION banking.guard_fx_amendment_immutable();
            CREATE TRIGGER trg_fx_amendment_approval_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_fx_amendment_approvals
                FOR EACH ROW EXECUTE FUNCTION banking.guard_fx_amendment_immutable();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE banking.bank_fx_amendment_approvals;
            DROP TABLE banking.bank_fx_amendment_preparations;
            DROP TABLE banking.bank_fx_revaluation_amendments;
            DROP FUNCTION banking.guard_fx_amendment_immutable();
            DROP INDEX IF EXISTS banking.ux_bank_fx_original_tenant;
            """);
    }
}
