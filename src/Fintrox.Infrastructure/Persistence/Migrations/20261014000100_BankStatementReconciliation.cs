using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Fintrox.Infrastructure.Persistence;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>
/// Persistent base-currency bank reconciliation ledger.
/// Raw SQL tables are intentionally NOT EF aggregates: every operation uses
/// the same EF connection/transaction with explicitly parameterized SQL.
/// </summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261014000100_BankStatementReconciliation")]
public sealed class BankStatementReconciliation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE SCHEMA IF NOT EXISTS banking;

            CREATE TABLE banking.bank_accounts (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL REFERENCES core.organizations(id) ON DELETE RESTRICT,
                iban varchar(34) NOT NULL,
                name varchar(160) NOT NULL,
                currency_code varchar(3) NOT NULL,
                gl_account_id uuid NOT NULL,
                created_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_banking_accounts_tenant_identity UNIQUE (id, organization_id),
                CONSTRAINT uq_banking_accounts_tenant_iban UNIQUE (organization_id, iban),
                CONSTRAINT fk_bank_accounts_ledger FOREIGN KEY (gl_account_id, organization_id)
                    REFERENCES accounting.accounts(id, organization_id) ON DELETE RESTRICT
            );
            -- One distinct GL cash/bank control account per bank account:
            -- otherwise account-level bank-versus-ledger reconciliation is ambiguous.
            CREATE UNIQUE INDEX ux_bank_accounts_tenant_gl
                ON banking.bank_accounts(organization_id, gl_account_id);

            CREATE TABLE banking.bank_statements (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                bank_account_id uuid NOT NULL,
                external_id varchar(120) NOT NULL,
                from_date date NOT NULL,
                to_date date NOT NULL,
                opening_balance numeric(19,4) NOT NULL,
                closing_balance numeric(19,4) NOT NULL,
                payload_sha256 char(64) NOT NULL,
                imported_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_bank_statements_tenant_identity UNIQUE(id, organization_id),
                CONSTRAINT uq_bank_statements_external UNIQUE(organization_id, bank_account_id, external_id),
                CONSTRAINT ck_bank_statements_dates CHECK (from_date <= to_date),
                CONSTRAINT fk_bank_statements_account FOREIGN KEY(bank_account_id, organization_id)
                    REFERENCES banking.bank_accounts(id, organization_id) ON DELETE RESTRICT
            );
            CREATE INDEX ix_bank_statements_date
                ON banking.bank_statements(organization_id, bank_account_id, to_date);

            CREATE TABLE banking.bank_statement_lines (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                statement_id uuid NOT NULL,
                external_id varchar(120) NOT NULL,
                value_date date NOT NULL,
                signed_amount numeric(19,4) NOT NULL,
                description varchar(500) NOT NULL,
                matched_payment_id uuid NULL,
                matched_at_utc timestamptz NULL,
                CONSTRAINT uq_bank_lines_tenant_identity UNIQUE(id, organization_id),
                CONSTRAINT uq_bank_lines_external UNIQUE(organization_id, statement_id, external_id),
                CONSTRAINT uq_bank_lines_payment UNIQUE(organization_id, matched_payment_id),
                CONSTRAINT ck_bank_lines_nonzero CHECK(signed_amount <> 0),
                CONSTRAINT ck_bank_lines_match_consistent CHECK (
                    (matched_payment_id IS NULL AND matched_at_utc IS NULL)
                    OR (matched_payment_id IS NOT NULL AND matched_at_utc IS NOT NULL)),
                CONSTRAINT fk_bank_lines_statement FOREIGN KEY(statement_id, organization_id)
                    REFERENCES banking.bank_statements(id, organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_lines_payment FOREIGN KEY(matched_payment_id, organization_id)
                    REFERENCES payments.payments(id, organization_id) ON DELETE RESTRICT
            );
            CREATE INDEX ix_bank_lines_tenant_unmatched
                ON banking.bank_statement_lines(organization_id, statement_id)
                WHERE matched_payment_id IS NULL;

            -- Bank evidence is immutable after import and may only be enriched
            -- with one verified payment match; updates never change amounts.
            CREATE OR REPLACE FUNCTION banking.guard_statement_line_changes()
            RETURNS trigger LANGUAGE plpgsql AS $fn$
            BEGIN
                IF TG_OP = 'DELETE' THEN
                    RAISE EXCEPTION 'Imported bank evidence cannot be deleted';
                END IF;
                IF ROW(OLD.id, OLD.organization_id, OLD.statement_id, OLD.external_id,
                    OLD.value_date, OLD.signed_amount, OLD.description)
                    IS DISTINCT FROM
                   ROW(NEW.id, NEW.organization_id, NEW.statement_id, NEW.external_id,
                    NEW.value_date, NEW.signed_amount, NEW.description) THEN
                    RAISE EXCEPTION 'Imported bank evidence is immutable';
                END IF;
                IF OLD.matched_payment_id IS NOT NULL AND
                    (NEW.matched_payment_id IS DISTINCT FROM OLD.matched_payment_id
                     OR NEW.matched_at_utc IS DISTINCT FROM OLD.matched_at_utc) THEN
                    RAISE EXCEPTION 'Bank payment match cannot be changed';
                END IF;
                RETURN NEW;
            END
            $fn$;
            CREATE TRIGGER trg_banking_line_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_statement_lines
                FOR EACH ROW EXECUTE FUNCTION banking.guard_statement_line_changes();

            CREATE OR REPLACE FUNCTION banking.guard_statement_immutable()
            RETURNS trigger LANGUAGE plpgsql AS $fn$
            BEGIN
                RAISE EXCEPTION 'Imported bank statement header is immutable';
            END
            $fn$;
            CREATE TRIGGER trg_bank_statement_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_statements
                FOR EACH ROW EXECUTE FUNCTION banking.guard_statement_immutable();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS banking.bank_statement_lines;
            DROP TABLE IF EXISTS banking.bank_statements;
            DROP TABLE IF EXISTS banking.bank_accounts;
            DROP FUNCTION IF EXISTS banking.guard_statement_line_changes();
            DROP FUNCTION IF EXISTS banking.guard_statement_immutable();
            """);
    }
}
