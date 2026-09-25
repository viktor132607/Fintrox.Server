using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "purchases");

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "purchases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    internal_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    counterparty_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_document_number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    document_date = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    base_currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    exchange_rate = table.Column<decimal>(type: "numeric(22,10)", precision: 22, scale: 10, nullable: true),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    supplier_legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    supplier_registration_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    supplier_vat_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    supplier_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    supplier_address_line_1 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    supplier_address_line_2 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    supplier_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    supplier_postal_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    net_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    vat_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    recoverable_vat_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    non_recoverable_vat_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    gross_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    received_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                    table.UniqueConstraint("ak_purchase_documents_id_organization", x => new { x.id, x.organization_id });
                    table.CheckConstraint("ck_purchase_documents_dates", "due_date >= document_date");
                    table.CheckConstraint("ck_purchase_documents_exchange_rate", "exchange_rate IS NULL OR exchange_rate > 0");
                    table.CheckConstraint("ck_purchase_documents_invoice_supplier_number", "type <> 'Invoice' OR status = 'Draft' OR supplier_document_number IS NOT NULL");
                    table.CheckConstraint("ck_purchase_documents_lifecycle", "(status = 'Draft' AND internal_number IS NULL AND received_at_utc IS NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL) OR (status = 'Received' AND internal_number IS NOT NULL AND received_at_utc IS NOT NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL) OR (status = 'Cancelled' AND internal_number IS NOT NULL AND received_at_utc IS NOT NULL AND cancelled_at_utc IS NOT NULL AND cancellation_reason IS NOT NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL)");
                    table.CheckConstraint("ck_purchase_documents_totals", "net_total >= 0 AND vat_total >= 0 AND recoverable_vat_total >= 0 AND non_recoverable_vat_total >= 0 AND gross_total >= 0 AND net_total + vat_total = gross_total AND recoverable_vat_total + non_recoverable_vat_total = vat_total");
                    table.ForeignKey(
                        name: "FK_documents_counterparties_counterparty_id_organization_id",
                        columns: x => new { x.counterparty_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "counterparties",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documents_currencies_base_currency_id_organization_id",
                        columns: x => new { x.base_currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documents_currencies_currency_id_organization_id",
                        columns: x => new { x.currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documents_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_document_number_sequences",
                schema: "purchases",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    calendar_year = table.Column<int>(type: "integer", nullable: false),
                    last_number = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_document_number_sequences", x => new { x.organization_id, x.calendar_year });
                    table.CheckConstraint("ck_purchase_document_sequences_last_number", "last_number >= 0");
                    table.CheckConstraint("ck_purchase_document_sequences_year", "calendar_year >= 1");
                    table.ForeignKey(
                        name: "FK_purchase_document_number_sequences_organizations_organizati~",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_lines",
                schema: "purchases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    item_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    unit_of_measure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    unit_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    discount_percent = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    vat_code_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vat_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    vat_rate_percent = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    recoverable_vat_percent = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    net_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    recoverable_vat_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    non_recoverable_vat_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    gross_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_lines", x => x.id);
                    table.CheckConstraint("ck_purchase_document_lines_amounts", "net_amount >= 0 AND vat_amount >= 0 AND recoverable_vat_amount >= 0 AND non_recoverable_vat_amount >= 0 AND gross_amount >= 0 AND net_amount + vat_amount = gross_amount AND recoverable_vat_amount + non_recoverable_vat_amount = vat_amount");
                    table.CheckConstraint("ck_purchase_document_lines_discount", "discount_percent >= 0 AND discount_percent <= 100");
                    table.CheckConstraint("ck_purchase_document_lines_quantity", "quantity > 0");
                    table.CheckConstraint("ck_purchase_document_lines_recoverable_vat", "recoverable_vat_percent >= 0 AND recoverable_vat_percent <= 100");
                    table.CheckConstraint("ck_purchase_document_lines_unit_price", "unit_price >= 0");
                    table.CheckConstraint("ck_purchase_document_lines_vat_rate", "vat_rate_percent >= 0 AND vat_rate_percent <= 100");
                    table.ForeignKey(
                        name: "FK_document_lines_documents_purchase_document_id_organization_~",
                        columns: x => new { x.purchase_document_id, x.organization_id },
                        principalSchema: "purchases",
                        principalTable: "documents",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_lines_vat_codes_vat_code_id_organization_id",
                        columns: x => new { x.vat_code_id, x.organization_id },
                        principalSchema: "tax",
                        principalTable: "vat_codes",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_lines_purchase_document_id_organization_id",
                schema: "purchases",
                table: "document_lines",
                columns: new[] { "purchase_document_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_document_lines_vat_code_id_organization_id",
                schema: "purchases",
                table: "document_lines",
                columns: new[] { "vat_code_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_document_lines_organization_vat_code",
                schema: "purchases",
                table: "document_lines",
                columns: new[] { "organization_id", "vat_code_id" });

            migrationBuilder.CreateIndex(
                name: "ux_purchase_document_lines_document_line_number",
                schema: "purchases",
                table: "document_lines",
                columns: new[] { "organization_id", "purchase_document_id", "line_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_base_currency_id_organization_id",
                schema: "purchases",
                table: "documents",
                columns: new[] { "base_currency_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_counterparty_id_organization_id",
                schema: "purchases",
                table: "documents",
                columns: new[] { "counterparty_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_currency_id_organization_id",
                schema: "purchases",
                table: "documents",
                columns: new[] { "currency_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_documents_organization_date",
                schema: "purchases",
                table: "documents",
                columns: new[] { "organization_id", "document_date" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_documents_organization_status_date",
                schema: "purchases",
                table: "documents",
                columns: new[] { "organization_id", "status", "document_date" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_documents_organization_type_date",
                schema: "purchases",
                table: "documents",
                columns: new[] { "organization_id", "type", "document_date" });

            migrationBuilder.CreateIndex(
                name: "ux_purchase_documents_organization_internal_number",
                schema: "purchases",
                table: "documents",
                columns: new[] { "organization_id", "internal_number" },
                unique: true,
                filter: "internal_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_purchase_documents_supplier_document_number",
                schema: "purchases",
                table: "documents",
                columns: new[] { "organization_id", "counterparty_id", "supplier_document_number" },
                unique: true,
                filter: "supplier_document_number IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_lines",
                schema: "purchases");

            migrationBuilder.DropTable(
                name: "purchase_document_number_sequences",
                schema: "purchases");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "purchases");
        }
    }
}
