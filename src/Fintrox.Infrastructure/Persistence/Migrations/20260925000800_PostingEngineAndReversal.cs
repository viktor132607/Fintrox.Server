using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PostingEngineAndReversal : Migration
    {
        private static readonly string[] IdOrganizationColumns =
            ["id", "organization_id"];

        private static readonly string[] FiscalYearOrganizationColumns =
            ["fiscal_year_id", "organization_id"];

        private static readonly string[] ReversalOrganizationColumns =
            ["reversal_of_journal_entry_id", "organization_id"];

        private static readonly string[] ReversedByOrganizationColumns =
            ["reversed_by_journal_entry_id", "organization_id"];

        private static readonly string[] OrganizationNumberColumns =
            ["organization_id", "number"];

        private static readonly string[] OrganizationReversalColumns =
            ["organization_id", "reversal_of_journal_entry_id"];

        private static readonly string[] OrganizationReversedByColumns =
            ["organization_id", "reversed_by_journal_entry_id"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "reversal_of_journal_entry_id",
                schema: "accounting",
                table: "journal_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "reversed_by_journal_entry_id",
                schema: "accounting",
                table: "journal_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "journal_number_sequences",
                schema: "accounting",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fiscal_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_number = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_number_sequences", x => new { x.organization_id, x.fiscal_year_id });
                    table.ForeignKey(
                        name: "FK_journal_number_sequences_fiscal_years_fiscal_year_id_organi~",
                        columns: x => new { x.fiscal_year_id, x.organization_id },
                        principalSchema: "accounting",
                        principalTable: "fiscal_years",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_reversal_of_journal_entry_id_organization_id",
                schema: "accounting",
                table: "journal_entries",
                columns: ReversalOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_reversed_by_journal_entry_id_organization_id",
                schema: "accounting",
                table: "journal_entries",
                columns: ReversedByOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ux_journal_entries_organization_number",
                schema: "accounting",
                table: "journal_entries",
                columns: OrganizationNumberColumns,
                unique: true,
                filter: "number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_journal_entries_reversal_of",
                schema: "accounting",
                table: "journal_entries",
                columns: OrganizationReversalColumns,
                unique: true,
                filter: "reversal_of_journal_entry_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_journal_entries_reversed_by",
                schema: "accounting",
                table: "journal_entries",
                columns: OrganizationReversedByColumns,
                unique: true,
                filter: "reversed_by_journal_entry_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_journal_entries_posting_state",
                schema: "accounting",
                table: "journal_entries",
                sql: "(status = 'Draft' AND number IS NULL AND posted_at_utc IS NULL) OR (status IN ('Posted', 'Reversed') AND number IS NOT NULL AND posted_at_utc IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_journal_entries_reversed_link",
                schema: "accounting",
                table: "journal_entries",
                sql: "status <> 'Reversed' OR reversed_by_journal_entry_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_journal_number_sequences_fiscal_year_id_organization_id",
                schema: "accounting",
                table: "journal_number_sequences",
                columns: FiscalYearOrganizationColumns);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_journal_entries_reversal_of_journal_entry_i~",
                schema: "accounting",
                table: "journal_entries",
                columns: ReversalOrganizationColumns,
                principalSchema: "accounting",
                principalTable: "journal_entries",
                principalColumns: IdOrganizationColumns,
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_journal_entries_reversed_by_journal_entry_i~",
                schema: "accounting",
                table: "journal_entries",
                columns: ReversedByOrganizationColumns,
                principalSchema: "accounting",
                principalTable: "journal_entries",
                principalColumns: IdOrganizationColumns,
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION accounting.enforce_journal_entry_immutability()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.status IN ('Posted', 'Reversed') THEN
                            RAISE EXCEPTION 'Posted or reversed journal entries are immutable';
                        END IF;

                        RETURN OLD;
                    END IF;

                    IF OLD.status = 'Draft' THEN
                        IF NEW.status = 'Draft' THEN
                            RETURN NEW;
                        END IF;

                        IF NEW.status = 'Posted'
                           AND NEW.organization_id = OLD.organization_id
                           AND NEW.posting_date = OLD.posting_date
                           AND NEW.document_date = OLD.document_date
                           AND NEW.description = OLD.description
                           AND NEW.source = OLD.source
                           AND NEW.external_reference IS NOT DISTINCT FROM OLD.external_reference
                           AND NEW.fiscal_period_id = OLD.fiscal_period_id
                           AND NEW.reversal_of_journal_entry_id IS NOT DISTINCT FROM OLD.reversal_of_journal_entry_id
                           AND NEW.reversed_by_journal_entry_id IS NOT DISTINCT FROM OLD.reversed_by_journal_entry_id
                           AND NEW.created_at_utc = OLD.created_at_utc
                           AND NEW.created_by_user_id IS NOT DISTINCT FROM OLD.created_by_user_id THEN
                            RETURN NEW;
                        END IF;

                        RAISE EXCEPTION 'Invalid journal entry state transition from Draft';
                    END IF;

                    IF OLD.status = 'Posted' THEN
                        IF NEW.status = 'Reversed'
                           AND OLD.reversed_by_journal_entry_id IS NULL
                           AND NEW.reversed_by_journal_entry_id IS NOT NULL
                           AND NEW.organization_id = OLD.organization_id
                           AND NEW.number = OLD.number
                           AND NEW.posting_date = OLD.posting_date
                           AND NEW.document_date = OLD.document_date
                           AND NEW.description = OLD.description
                           AND NEW.source = OLD.source
                           AND NEW.external_reference IS NOT DISTINCT FROM OLD.external_reference
                           AND NEW.fiscal_period_id = OLD.fiscal_period_id
                           AND NEW.posted_at_utc = OLD.posted_at_utc
                           AND NEW.reversal_of_journal_entry_id IS NOT DISTINCT FROM OLD.reversal_of_journal_entry_id
                           AND NEW.created_at_utc = OLD.created_at_utc
                           AND NEW.created_by_user_id IS NOT DISTINCT FROM OLD.created_by_user_id THEN
                            RETURN NEW;
                        END IF;

                        RAISE EXCEPTION 'Posted journal entries are immutable';
                    END IF;

                    IF OLD.status = 'Reversed' THEN
                        RAISE EXCEPTION 'Reversed journal entries are immutable';
                    END IF;

                    RETURN NEW;
                END;
                $$;

                CREATE TRIGGER trg_journal_entries_immutability
                BEFORE UPDATE OR DELETE ON accounting.journal_entries
                FOR EACH ROW
                EXECUTE FUNCTION accounting.enforce_journal_entry_immutability();

                CREATE OR REPLACE FUNCTION accounting.enforce_journal_line_parent_draft()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    parent_status text;
                BEGIN
                    IF TG_OP = 'INSERT' THEN
                        SELECT status
                        INTO parent_status
                        FROM accounting.journal_entries
                        WHERE id = NEW.journal_entry_id
                          AND organization_id = NEW.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Journal lines can only be inserted under draft entries';
                        END IF;

                        RETURN NEW;
                    END IF;

                    IF TG_OP = 'UPDATE' THEN
                        SELECT status
                        INTO parent_status
                        FROM accounting.journal_entries
                        WHERE id = OLD.journal_entry_id
                          AND organization_id = OLD.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Journal lines under posted or reversed entries are immutable';
                        END IF;

                        SELECT status
                        INTO parent_status
                        FROM accounting.journal_entries
                        WHERE id = NEW.journal_entry_id
                          AND organization_id = NEW.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Journal lines can only be moved to draft entries';
                        END IF;

                        RETURN NEW;
                    END IF;

                    SELECT status
                    INTO parent_status
                    FROM accounting.journal_entries
                    WHERE id = OLD.journal_entry_id
                      AND organization_id = OLD.organization_id;

                    IF parent_status IS DISTINCT FROM 'Draft' THEN
                        RAISE EXCEPTION 'Journal lines under posted or reversed entries are immutable';
                    END IF;

                    RETURN OLD;
                END;
                $$;

                CREATE TRIGGER trg_journal_lines_parent_draft
                BEFORE INSERT OR UPDATE OR DELETE ON accounting.journal_lines
                FOR EACH ROW
                EXECUTE FUNCTION accounting.enforce_journal_line_parent_draft();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_journal_lines_parent_draft
                    ON accounting.journal_lines;

                DROP FUNCTION IF EXISTS accounting.enforce_journal_line_parent_draft();

                DROP TRIGGER IF EXISTS trg_journal_entries_immutability
                    ON accounting.journal_entries;

                DROP FUNCTION IF EXISTS accounting.enforce_journal_entry_immutability();
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entries_journal_entries_reversal_of_journal_entry_i~",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entries_journal_entries_reversed_by_journal_entry_i~",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropTable(
                name: "journal_number_sequences",
                schema: "accounting");

            migrationBuilder.DropIndex(
                name: "IX_journal_entries_reversal_of_journal_entry_id_organization_id",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropIndex(
                name: "IX_journal_entries_reversed_by_journal_entry_id_organization_id",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropIndex(
                name: "ux_journal_entries_organization_number",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropIndex(
                name: "ux_journal_entries_reversal_of",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropIndex(
                name: "ux_journal_entries_reversed_by",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropCheckConstraint(
                name: "ck_journal_entries_posting_state",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropCheckConstraint(
                name: "ck_journal_entries_reversed_link",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropColumn(
                name: "reversal_of_journal_entry_id",
                schema: "accounting",
                table: "journal_entries");

            migrationBuilder.DropColumn(
                name: "reversed_by_journal_entry_id",
                schema: "accounting",
                table: "journal_entries");
        }
    }
}
