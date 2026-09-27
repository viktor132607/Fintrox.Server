using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VatCurrenciesExchangeRates : Migration
    {
        private static readonly string[] IdOrganizationColumns =
            ["id", "organization_id"];

        private static readonly string[] OrganizationActiveColumns =
            ["organization_id", "is_active"];

        private static readonly string[] OrganizationCodeColumns =
            ["organization_id", "code"];

        private static readonly string[] BaseCurrencyOrganizationColumns =
            ["base_currency_id", "organization_id"];

        private static readonly string[] OrganizationDateColumns =
            ["organization_id", "effective_date"];

        private static readonly string[] QuoteCurrencyOrganizationColumns =
            ["quote_currency_id", "organization_id"];

        private static readonly string[] ExchangeRatePairDateColumns =
            ["organization_id", "base_currency_id", "quote_currency_id", "effective_date"];

        private static readonly string[] VatValidityColumns =
            ["organization_id", "is_active", "valid_from", "valid_to"];

        private static readonly string[] VatCodeValidFromColumns =
            ["organization_id", "code", "valid_from"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tax");

            migrationBuilder.CreateTable(
                name: "currencies",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    symbol = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    decimal_places = table.Column<int>(type: "integer", nullable: false),
                    is_base_currency = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.id);
                    table.UniqueConstraint("ak_currencies_id_organization", x => new { x.id, x.organization_id });
                    table.CheckConstraint("ck_currencies_base_active", "NOT is_base_currency OR is_active");
                    table.CheckConstraint("ck_currencies_decimal_places", "decimal_places >= 0 AND decimal_places <= 4");
                    table.ForeignKey(
                        name: "FK_currencies_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vat_codes",
                schema: "tax",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    rate_percent = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    applies_to_sales = table.Column<bool>(type: "boolean", nullable: false),
                    applies_to_purchases = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vat_codes", x => x.id);
                    table.UniqueConstraint("ak_vat_codes_id_organization", x => new { x.id, x.organization_id });
                    table.CheckConstraint("ck_vat_codes_applicability", "applies_to_sales OR applies_to_purchases");
                    table.CheckConstraint("ck_vat_codes_kind_rate", "(kind IN ('ZeroRated', 'Exempt', 'OutOfScope') AND rate_percent = 0) OR (kind IN ('Standard', 'Reduced') AND rate_percent > 0) OR kind = 'ReverseCharge'");
                    table.CheckConstraint("ck_vat_codes_rate", "rate_percent >= 0 AND rate_percent <= 100");
                    table.CheckConstraint("ck_vat_codes_validity", "valid_to IS NULL OR valid_to >= valid_from");
                    table.ForeignKey(
                        name: "FK_vat_codes_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quote_currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    rate = table.Column<decimal>(type: "numeric(22,10)", precision: 22, scale: 10, nullable: false),
                    source = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange_rates", x => x.id);
                    table.CheckConstraint("ck_exchange_rates_distinct_currencies", "base_currency_id <> quote_currency_id");
                    table.CheckConstraint("ck_exchange_rates_positive_rate", "rate > 0");
                    table.ForeignKey(
                        name: "FK_exchange_rates_currencies_base_currency_id_organization_id",
                        columns: x => new { x.base_currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exchange_rates_currencies_quote_currency_id_organization_id",
                        columns: x => new { x.quote_currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exchange_rates_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_currencies_organization_active",
                schema: "core",
                table: "currencies",
                columns: OrganizationActiveColumns);

            migrationBuilder.CreateIndex(
                name: "ux_currencies_organization_base",
                schema: "core",
                table: "currencies",
                column: "organization_id",
                unique: true,
                filter: "is_base_currency");

            migrationBuilder.CreateIndex(
                name: "ux_currencies_organization_code",
                schema: "core",
                table: "currencies",
                columns: OrganizationCodeColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_base_currency_id_organization_id",
                schema: "accounting",
                table: "exchange_rates",
                columns: BaseCurrencyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_organization_date",
                schema: "accounting",
                table: "exchange_rates",
                columns: OrganizationDateColumns);

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_quote_currency_id_organization_id",
                schema: "accounting",
                table: "exchange_rates",
                columns: QuoteCurrencyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ux_exchange_rates_pair_date",
                schema: "accounting",
                table: "exchange_rates",
                columns: ExchangeRatePairDateColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vat_codes_organization_validity",
                schema: "tax",
                table: "vat_codes",
                columns: VatValidityColumns);

            migrationBuilder.CreateIndex(
                name: "ux_vat_codes_organization_code_valid_from",
                schema: "tax",
                table: "vat_codes",
                columns: VatCodeValidFromColumns,
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exchange_rates",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "vat_codes",
                schema: "tax");

            migrationBuilder.DropTable(
                name: "currencies",
                schema: "core");
        }
    }
}
