using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChartOfAccounts : Migration
    {
        private static readonly string[] OrganizationParentIndexColumns =
            ["organization_id", "parent_account_id"];

        private static readonly string[] OrganizationTypeActiveIndexColumns =
            ["organization_id", "type", "is_active"];

        private static readonly string[] OrganizationCodeIndexColumns =
            ["organization_id", "code"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "accounting");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    parent_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_analytical = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    allow_manual_posting = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_accounts_accounts_parent_account_id",
                        column: x => x.parent_account_id,
                        principalSchema: "accounting",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_accounts_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_organization_parent",
                schema: "accounting",
                table: "accounts",
                columns: OrganizationParentIndexColumns);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_organization_type_active",
                schema: "accounting",
                table: "accounts",
                columns: OrganizationTypeActiveIndexColumns);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_parent_account_id",
                schema: "accounting",
                table: "accounts",
                column: "parent_account_id");

            migrationBuilder.CreateIndex(
                name: "ux_accounts_organization_code",
                schema: "accounting",
                table: "accounts",
                columns: OrganizationCodeIndexColumns,
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts",
                schema: "accounting");
        }
    }
}
