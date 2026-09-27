using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FiscalCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fiscal_years",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_years", x => x.id);
                    table.UniqueConstraint("ak_fiscal_years_id_organization", x => new { x.id, x.organization_id });
                    table.CheckConstraint("ck_fiscal_years_date_range", "end_date >= start_date");
                    table.ForeignKey(
                        name: "FK_fiscal_years_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accounting_periods",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fiscal_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounting_periods", x => x.id);
                    table.CheckConstraint("ck_accounting_periods_date_range", "end_date >= start_date");
                    table.CheckConstraint("ck_accounting_periods_number", "number >= 1 AND number <= 99");
                    table.ForeignKey(
                        name: "FK_accounting_periods_fiscal_years_fiscal_year_id_organization~",
                        columns: x => new { x.fiscal_year_id, x.organization_id },
                        principalSchema: "accounting",
                        principalTable: "fiscal_years",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounting_periods_fiscal_year_id_organization_id",
                schema: "accounting",
                table: "accounting_periods",
                columns: new[] { "fiscal_year_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "ix_accounting_periods_organization_status",
                schema: "accounting",
                table: "accounting_periods",
                columns: new[] { "organization_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_accounting_periods_year_range",
                schema: "accounting",
                table: "accounting_periods",
                columns: new[] { "organization_id", "fiscal_year_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "ux_accounting_periods_year_number",
                schema: "accounting",
                table: "accounting_periods",
                columns: new[] { "organization_id", "fiscal_year_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_years_organization_range",
                schema: "accounting",
                table: "fiscal_years",
                columns: new[] { "organization_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "ux_fiscal_years_organization_name",
                schema: "accounting",
                table: "fiscal_years",
                columns: new[] { "organization_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounting_periods",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "fiscal_years",
                schema: "accounting");
        }
    }
}
