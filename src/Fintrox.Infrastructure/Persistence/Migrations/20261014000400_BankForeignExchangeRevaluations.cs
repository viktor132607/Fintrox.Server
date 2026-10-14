using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>Immutable, unique period-end valuation evidence. Zero adjustment
/// records deliberately have no journal, but remain auditable and idempotent.</summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261014000400_BankForeignExchangeRevaluations")]
public sealed class BankForeignExchangeRevaluations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE banking.bank_fx_revaluations (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                bank_account_id uuid NOT NULL,
                statement_id uuid NOT NULL,
                cutoff_date date NOT NULL,
                exchange_rate_id uuid NOT NULL,
                approved_rate numeric(22,10) NOT NULL,
                native_closing_balance numeric(19,4) NOT NULL,
                ledger_base_before numeric(19,4) NOT NULL,
                target_base_balance numeric(19,4) NOT NULL,
                adjustment_base numeric(19,4) NOT NULL,
                gain_account_id uuid NOT NULL,
                loss_account_id uuid NOT NULL,
                journal_entry_id uuid NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_bank_fx_revaluation_period
                    UNIQUE(organization_id,bank_account_id,cutoff_date),
                CONSTRAINT uq_bank_fx_revaluation_statement
                    UNIQUE(organization_id,statement_id),
                CONSTRAINT uq_bank_fx_revaluation_journal
                    UNIQUE(organization_id,journal_entry_id),
                CONSTRAINT fk_bank_fx_revaluation_account
                    FOREIGN KEY(bank_account_id,organization_id)
                    REFERENCES banking.bank_accounts(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_revaluation_statement
                    FOREIGN KEY(statement_id,organization_id)
                    REFERENCES banking.bank_statements(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_revaluation_rate
                    FOREIGN KEY(exchange_rate_id,organization_id)
                    REFERENCES accounting.exchange_rates(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_revaluation_gain
                    FOREIGN KEY(gain_account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_revaluation_loss
                    FOREIGN KEY(loss_account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_revaluation_journal
                    FOREIGN KEY(journal_entry_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT ck_bank_fx_revaluation_rate CHECK(approved_rate>0),
                CONSTRAINT ck_bank_fx_revaluation_zero_journal CHECK(
                    (adjustment_base=0 AND journal_entry_id IS NULL)
                    OR (adjustment_base<>0 AND journal_entry_id IS NOT NULL)),
                CONSTRAINT ck_bank_fx_revaluation_math CHECK(
                    adjustment_base=target_base_balance-ledger_base_before)
            );

            CREATE OR REPLACE FUNCTION banking.guard_fx_revaluation_immutable()
            RETURNS trigger LANGUAGE plpgsql AS $fn$
            BEGIN
                RAISE EXCEPTION 'Bank FX period-end revaluation evidence is immutable';
            END $fn$;
            CREATE TRIGGER trg_bank_fx_revaluation_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_fx_revaluations
                FOR EACH ROW EXECUTE FUNCTION banking.guard_fx_revaluation_immutable();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE banking.bank_fx_revaluations;
            DROP FUNCTION banking.guard_fx_revaluation_immutable();
            """);
    }
}
