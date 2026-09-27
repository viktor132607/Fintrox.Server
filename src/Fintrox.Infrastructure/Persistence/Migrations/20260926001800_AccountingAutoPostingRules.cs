using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccountingAutoPostingRules : Migration
    {
        private static readonly string[] IdOrganizationColumns =
            ["id", "organization_id"];

        private static readonly string[] OrganizationSourceReferenceColumns =
            ["organization_id", "source", "external_reference"];

        private static readonly string[] AccountOrganizationColumns =
            ["account_id", "organization_id"];

        private static readonly string[] ActiveComponentColumns =
            ["organization_id", "is_active", "component"];

        private static readonly string[] ResolutionColumns =
            ["organization_id", "component", "match_kind", "match_value"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auto_posting_rules",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    component = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    match_kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    match_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auto_posting_rules", x => x.id);
                    table.UniqueConstraint("ak_auto_posting_rules_id_organization", x => new { x.id, x.organization_id });
                    table.ForeignKey(
                        name: "FK_auto_posting_rules_accounts_account_id_organization_id",
                        columns: x => new { x.account_id, x.organization_id },
                        principalSchema: "accounting",
                        principalTable: "accounts",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_auto_posting_rules_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_journal_entries_system_external_reference",
                schema: "accounting",
                table: "journal_entries",
                columns: OrganizationSourceReferenceColumns,
                unique: true,
                filter: "source = 'System' AND external_reference IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_auto_posting_rules_account_id_organization_id",
                schema: "accounting",
                table: "auto_posting_rules",
                columns: AccountOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_auto_posting_rules_active_component",
                schema: "accounting",
                table: "auto_posting_rules",
                columns: ActiveComponentColumns);

            migrationBuilder.CreateIndex(
                name: "ux_auto_posting_rules_resolution",
                schema: "accounting",
                table: "auto_posting_rules",
                columns: ResolutionColumns,
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auto_posting_rules",
                schema: "accounting");

            migrationBuilder.DropIndex(
                name: "ux_journal_entries_system_external_reference",
                schema: "accounting",
                table: "journal_entries");
        }
    }
}
