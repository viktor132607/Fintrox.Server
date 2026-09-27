using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsAndAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payments");

            migrationBuilder.CreateTable(
                name: "payment_number_sequences",
                schema: "payments",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    calendar_year = table.Column<int>(type: "integer", nullable: false),
                    direction = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    last_number = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_number_sequences", x => new { x.organization_id, x.calendar_year, x.direction });
                    table.CheckConstraint("ck_payment_sequences_last_number", "last_number >= 0");
                    table.CheckConstraint("ck_payment_sequences_year", "calendar_year >= 1");
                    table.ForeignKey(
                        name: "FK_payment_number_sequences_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    internal_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    direction = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    counterparty_id = table.Column<Guid>(type: "uuid", nullable: false),
                    counterparty_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    counterparty_registration_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    counterparty_vat_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    method = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    base_currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    exchange_rate = table.Column<decimal>(type: "numeric(22,10)", precision: 22, scale: 10, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    allocated_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    reference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    confirmed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.UniqueConstraint("ak_payments_id_organization", x => new { x.id, x.organization_id });
                    table.CheckConstraint("ck_payments_allocated_amount", "allocated_amount >= 0 AND allocated_amount <= amount");
                    table.CheckConstraint("ck_payments_amount", "amount > 0");
                    table.CheckConstraint("ck_payments_exchange_rate", "exchange_rate IS NULL OR exchange_rate > 0");
                    table.CheckConstraint("ck_payments_lifecycle", "(status = 'Draft' AND internal_number IS NULL AND base_currency_id IS NULL AND base_currency_code IS NULL AND exchange_rate IS NULL AND confirmed_at_utc IS NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL) OR (status = 'Confirmed' AND internal_number IS NOT NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL AND confirmed_at_utc IS NOT NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL) OR (status = 'Cancelled' AND internal_number IS NOT NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL AND confirmed_at_utc IS NOT NULL AND cancelled_at_utc IS NOT NULL AND cancellation_reason IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_payments_counterparties_counterparty_id_organization_id",
                        columns: x => new { x.counterparty_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "counterparties",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_currencies_base_currency_id_organization_id",
                        columns: x => new { x.base_currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_currencies_currency_id_organization_id",
                        columns: x => new { x.currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "allocations",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    target_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    sales_invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    purchase_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    document_number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    document_currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    document_exchange_rate = table.Column<decimal>(type: "numeric(22,10)", precision: 22, scale: 10, nullable: false),
                    document_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    payment_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allocations", x => x.id);
                    table.CheckConstraint("ck_payment_allocations_amounts", "document_amount > 0 AND payment_amount > 0");
                    table.CheckConstraint("ck_payment_allocations_exchange_rate", "document_exchange_rate > 0");
                    table.CheckConstraint("ck_payment_allocations_target", "(target_type = 'SalesInvoice' AND sales_invoice_id IS NOT NULL AND purchase_document_id IS NULL) OR (target_type = 'PurchaseDocument' AND purchase_document_id IS NOT NULL AND sales_invoice_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_allocations_currencies_document_currency_id_organization_id",
                        columns: x => new { x.document_currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_allocations_documents_purchase_document_id_organization_id",
                        columns: x => new { x.purchase_document_id, x.organization_id },
                        principalSchema: "purchases",
                        principalTable: "documents",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_allocations_invoices_sales_invoice_id_organization_id",
                        columns: x => new { x.sales_invoice_id, x.organization_id },
                        principalSchema: "sales",
                        principalTable: "invoices",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_allocations_payments_payment_id_organization_id",
                        columns: x => new { x.payment_id, x.organization_id },
                        principalSchema: "payments",
                        principalTable: "payments",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_allocations_document_currency_id_organization_id",
                schema: "payments",
                table: "allocations",
                columns: new[] { "document_currency_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_allocations_payment_id_organization_id",
                schema: "payments",
                table: "allocations",
                columns: new[] { "payment_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_allocations_purchase_document_id_organization_id",
                schema: "payments",
                table: "allocations",
                columns: new[] { "purchase_document_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_allocations_sales_invoice_id_organization_id",
                schema: "payments",
                table: "allocations",
                columns: new[] { "sales_invoice_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_allocations_purchase_document",
                schema: "payments",
                table: "allocations",
                columns: new[] { "organization_id", "purchase_document_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_allocations_sales_invoice",
                schema: "payments",
                table: "allocations",
                columns: new[] { "organization_id", "sales_invoice_id" });

            migrationBuilder.CreateIndex(
                name: "ux_payment_allocations_payment_line_number",
                schema: "payments",
                table: "allocations",
                columns: new[] { "organization_id", "payment_id", "line_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_payment_allocations_payment_purchase_document",
                schema: "payments",
                table: "allocations",
                columns: new[] { "organization_id", "payment_id", "purchase_document_id" },
                unique: true,
                filter: "purchase_document_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_payment_allocations_payment_sales_invoice",
                schema: "payments",
                table: "allocations",
                columns: new[] { "organization_id", "payment_id", "sales_invoice_id" },
                unique: true,
                filter: "sales_invoice_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payments_base_currency_id_organization_id",
                schema: "payments",
                table: "payments",
                columns: new[] { "base_currency_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_payments_counterparty_id_organization_id",
                schema: "payments",
                table: "payments",
                columns: new[] { "counterparty_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_payments_currency_id_organization_id",
                schema: "payments",
                table: "payments",
                columns: new[] { "currency_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_organization_counterparty_date",
                schema: "payments",
                table: "payments",
                columns: new[] { "organization_id", "counterparty_id", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_organization_date",
                schema: "payments",
                table: "payments",
                columns: new[] { "organization_id", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_organization_status_date",
                schema: "payments",
                table: "payments",
                columns: new[] { "organization_id", "status", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "ux_payments_organization_internal_number",
                schema: "payments",
                table: "payments",
                columns: new[] { "organization_id", "internal_number" },
                unique: true,
                filter: "internal_number IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "allocations",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "payment_number_sequences",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "payments");
        }
    }
}
