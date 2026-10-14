using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>
/// Immutable maker/checker approvals for bank FX cutoff reconciliation.
/// These are internal approvals, not bank-issued cryptographic signatures.
/// </summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261014000500_ForeignBankCloseApprovals")]
public sealed class ForeignBankCloseApprovals : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE banking.bank_fx_close_preparations (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                statement_id uuid NOT NULL,
                bank_account_id uuid NOT NULL,
                cutoff_date date NOT NULL,
                prepared_by_user_id uuid NOT NULL,
                evidence_sha256 char(64) NOT NULL,
                native_closing_balance numeric(19,4) NOT NULL,
                expected_ledger_base numeric(19,4) NOT NULL,
                review_note varchar(500) NOT NULL,
                prepared_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_bank_fx_close_prep_tenant_statement UNIQUE(organization_id,statement_id),
                CONSTRAINT uq_bank_fx_close_prep_tenant_id UNIQUE(id,organization_id),
                CONSTRAINT fk_bank_fx_close_prep_statement FOREIGN KEY(statement_id,organization_id)
                    REFERENCES banking.bank_statements(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT fk_bank_fx_close_prep_account FOREIGN KEY(bank_account_id,organization_id)
                    REFERENCES banking.bank_accounts(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT ck_bank_fx_close_prep_note CHECK(length(trim(review_note)) BETWEEN 10 AND 500)
            );

            CREATE TABLE banking.bank_fx_close_approvals (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                preparation_id uuid NOT NULL,
                approved_by_user_id uuid NOT NULL,
                evidence_sha256 char(64) NOT NULL,
                approval_note varchar(500) NOT NULL,
                approved_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_bank_fx_close_approval UNIQUE(organization_id,preparation_id),
                CONSTRAINT fk_bank_fx_close_approval_preparation FOREIGN KEY(preparation_id,organization_id)
                    REFERENCES banking.bank_fx_close_preparations(id,organization_id) ON DELETE RESTRICT,
                CONSTRAINT ck_bank_fx_close_approval_note CHECK(length(trim(approval_note)) BETWEEN 10 AND 500)
            );

            CREATE OR REPLACE FUNCTION banking.guard_bank_close_immutable()
            RETURNS trigger LANGUAGE plpgsql AS $fn$
            BEGIN
                RAISE EXCEPTION 'Bank close review evidence is immutable';
            END $fn$;
            CREATE TRIGGER trg_bank_fx_close_preparations_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_fx_close_preparations
                FOR EACH ROW EXECUTE FUNCTION banking.guard_bank_close_immutable();
            CREATE TRIGGER trg_bank_fx_close_approvals_immutable
                BEFORE UPDATE OR DELETE ON banking.bank_fx_close_approvals
                FOR EACH ROW EXECUTE FUNCTION banking.guard_bank_close_immutable();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE banking.bank_fx_close_approvals;
            DROP TABLE banking.bank_fx_close_preparations;
            DROP FUNCTION banking.guard_bank_close_immutable();
            """);
    }
}
