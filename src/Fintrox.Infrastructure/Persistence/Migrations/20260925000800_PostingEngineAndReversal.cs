using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PostingEngineAndReversal : Migration
    {
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
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_reversal_of_journal_entry_id_organization_id",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "reversal_of_journal_entry_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_reversed_by_journal_entry_id_organization_id",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "reversed_by_journal_entry_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "ux_journal_entries_organization_number",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "organization_id", "number" },
                unique: true,
                filter: "number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_journal_entries_reversal_of",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "organization_id", "reversal_of_journal_entry_id" },
                unique: true,
                filter: "reversal_of_journal_entry_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_journal_entries_reversed_by",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "organization_id", "reversed_by_journal_entry_id" },
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
                columns: new[] { "fiscal_year_id", "organization_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_journal_entries_reversal_of_journal_entry_i~",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "reversal_of_journal_entry_id", "organization_id" },
                principalSchema: "accounting",
                principalTable: "journal_entries",
                principalColumns: new[] { "id", "organization_id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_journal_entries_reversed_by_journal_entry_i~",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "reversed_by_journal_entry_id", "organization_id" },
                principalSchema: "accounting",
                principalTable: "journal_entries",
                principalColumns: new[] { "id", "organization_id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
