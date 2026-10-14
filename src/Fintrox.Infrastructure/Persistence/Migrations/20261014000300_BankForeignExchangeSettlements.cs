using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>
/// Exact FX rate snapshot and immutable one-to-one statement line to
/// realized FX settlement journal. The GL remains base-currency valued.
/// </summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261014000300_BankForeignExchangeSettlements")]
public sealed class BankForeignExchangeSettlements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            -- Enforce that the approved FX rate belongs to the same tenant,
            -- including for non-API database writers.
            CREATE UNIQUE INDEX IF NOT EXISTS ux_fx_rate_tenant_identity
                ON accounting.exchange_rates(id,organization_id);

            CREATE TABLE banking.bank_fx_settlements (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                statement_line_id uuid NOT NULL,
                journal_entry_id uuid NOT NULL,
                exchange_rate_id uuid NOT NULL,
                approved_rate numeric(22,10) NOT NULL,
                clearing_account_id uuid NOT NULL,
                clearing_base_amount numeric(19,4) NOT NULL,
                bank_base_amount numeric(19,4) NOT NULL,
                realized_gain_base numeric(19,4) NOT NULL DEFAULT 0,
                realized_loss_base numeric(19,4) NOT NULL DEFAULT 0,
                gain_account_id uuid NOT NULL,
                loss_account_id uuid NOT NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_bank_fx_line UNIQUE(organization_id,statement_line_id),
                CONSTRAINT uq_bank_fx_journal UNIQUE(organization_id,journal_entry_id),
                CONSTRAINT fk_bank_fx_line FOREIGN KEY(statement_line_id,organization_id)
                    REFERENCES banking.bank_statement_lines(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_journal FOREIGN KEY(journal_entry_id,organization_id)
                    REFERENCES accounting.journal_entries(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_clearing FOREIGN KEY(clearing_account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_gain FOREIGN KEY(gain_account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_loss FOREIGN KEY(loss_account_id,organization_id)
                    REFERENCES accounting.accounts(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_rate FOREIGN KEY(exchange_rate_id,organization_id)
                    REFERENCES accounting.exchange_rates(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT ck_bank_fx_amounts CHECK (
                    approved_rate > 0 AND clearing_base_amount > 0
                    AND bank_base_amount > 0
                    AND realized_gain_base >= 0 AND realized_loss_base >= 0
                    AND NOT(realized_gain_base > 0 AND realized_loss_base > 0)),
                CONSTRAINT ck_bank_fx_balances CHECK (
                    abs(bank_base_amount-clearing_base_amount) =
                       realized_gain_base+realized_loss_base)
            );

            CREATE OR REPLACE FUNCTION banking.guard_fx_settlement_immutable()
            RETURNS trigger LANGUAGE plpgsql AS $fn$
            BEGIN
                RAISE EXCEPTION 'Posted FX settlement audit evidence is immutable';
            END $fn$;
            CREATE TRIGGER trg_bank_fx_settlement_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_fx_settlements
                FOR EACH ROW EXECUTE FUNCTION banking.guard_fx_settlement_immutable();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE banking.bank_fx_settlements;
            DROP FUNCTION banking.guard_fx_settlement_immutable();
            DROP INDEX IF EXISTS accounting.ux_fx_rate_tenant_identity;
            """);
    }
}
