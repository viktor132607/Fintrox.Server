using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DoubleEntryJournal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "ak_accounts_id_organization",
                schema: "accounting",
                table: "accounts",
                columns: new[] { "id", "organization_id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_accounting_periods_id_organization",
                schema: "accounting",
                table: "accounting_periods",
                columns: new[] { "id", "organization_id" });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    posting_date = table.Column<DateOnly>(type: "date", nullable: false),
                    document_date = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    external_reference = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    fiscal_period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_entries", x => x.id);
                    table.UniqueConstraint("ak_journal_entries_id_organization", x => new { x.id, x.organization_id });
                    table.ForeignKey(
                        name: "FK_journal_entries_accounting_periods_fiscal_period_id_organiz~",
                        columns: x => new { x.fiscal_period_id, x.organization_id },
                        principalSchema: "accounting",
                        principalTable: "accounting_periods",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_journal_entries_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journal_lines",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    journal_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    credit = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_lines", x => x.id);
                    table.CheckConstraint("ck_journal_lines_single_sided_amount", "debit >= 0 AND credit >= 0 AND ((debit > 0 AND credit = 0) OR (credit > 0 AND debit = 0))");
                    table.ForeignKey(
                        name: "FK_journal_lines_accounts_account_id_organization_id",
                        columns: x => new { x.account_id, x.organization_id },
                        principalSchema: "accounting",
                        principalTable: "accounts",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_journal_lines_journal_entries_journal_entry_id_organization~",
                        columns: x => new { x.journal_entry_id, x.organization_id },
                        principalSchema: "accounting",
                        principalTable: "journal_entries",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_fiscal_period_id_organization_id",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "fiscal_period_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_organization_external_reference",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "organization_id", "external_reference" });

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_organization_posting_date",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "organization_id", "posting_date" });

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_organization_status_date",
                schema: "accounting",
                table: "journal_entries",
                columns: new[] { "organization_id", "status", "posting_date" });

            migrationBuilder.CreateIndex(
                name: "IX_journal_lines_account_id_organization_id",
                schema: "accounting",
                table: "journal_lines",
                columns: new[] { "account_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_journal_lines_journal_entry_id_organization_id",
                schema: "accounting",
                table: "journal_lines",
                columns: new[] { "journal_entry_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "ix_journal_lines_organization_account",
                schema: "accounting",
                table: "journal_lines",
                columns: new[] { "organization_id", "account_id" });

            migrationBuilder.CreateIndex(
                name: "ux_journal_lines_entry_line_number",
                schema: "accounting",
                table: "journal_lines",
                columns: new[] { "organization_id", "journal_entry_id", "line_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "journal_lines",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "journal_entries",
                schema: "accounting");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_accounts_id_organization",
                schema: "accounting",
                table: "accounts");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_accounting_periods_id_organization",
                schema: "accounting",
                table: "accounting_periods");
        }
    }
}
