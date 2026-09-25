using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Counterparties : Migration
    {
        private static readonly string[] OrganizationNameColumns =
            ["organization_id", "name"];

        private static readonly string[] OrganizationRolesActiveColumns =
            ["organization_id", "is_active", "is_customer", "is_supplier"];

        private static readonly string[] OrganizationCodeColumns =
            ["organization_id", "code"];

        private static readonly string[] OrganizationCountryRegistrationColumns =
            ["organization_id", "country_code", "registration_number"];

        private static readonly string[] OrganizationCountryVatColumns =
            ["organization_id", "country_code", "vat_number"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "counterparties",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    registration_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    vat_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    is_customer = table.Column<bool>(type: "boolean", nullable: false),
                    is_supplier = table.Column<bool>(type: "boolean", nullable: false),
                    payment_term_days = table.Column<int>(type: "integer", nullable: false),
                    contact_person = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    address_line_1 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    address_line_2 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counterparties", x => x.id);
                    table.UniqueConstraint("ak_counterparties_id_organization", x => new { x.id, x.organization_id });
                    table.CheckConstraint("ck_counterparties_role", "is_customer OR is_supplier");
                    table.ForeignKey(
                        name: "FK_counterparties_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_counterparties_organization_name",
                schema: "core",
                table: "counterparties",
                columns: OrganizationNameColumns);

            migrationBuilder.CreateIndex(
                name: "ix_counterparties_organization_roles_active",
                schema: "core",
                table: "counterparties",
                columns: OrganizationRolesActiveColumns);

            migrationBuilder.CreateIndex(
                name: "ux_counterparties_organization_code",
                schema: "core",
                table: "counterparties",
                columns: OrganizationCodeColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_counterparties_organization_country_registration",
                schema: "core",
                table: "counterparties",
                columns: OrganizationCountryRegistrationColumns,
                unique: true,
                filter: "registration_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_counterparties_organization_country_vat",
                schema: "core",
                table: "counterparties",
                columns: OrganizationCountryVatColumns,
                unique: true,
                filter: "vat_number IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "counterparties",
                schema: "core");
        }
    }
}
