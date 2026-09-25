using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SalesInvoices : Migration
    {
        private static readonly string[] IdOrganizationColumns =
            ["id", "organization_id"];

        private static readonly string[] CounterpartyOrganizationColumns =
            ["counterparty_id", "organization_id"];

        private static readonly string[] CurrencyOrganizationColumns =
            ["currency_id", "organization_id"];

        private static readonly string[] BaseCurrencyOrganizationColumns =
            ["base_currency_id", "organization_id"];

        private static readonly string[] InvoiceOrganizationColumns =
            ["sales_invoice_id", "organization_id"];

        private static readonly string[] VatCodeOrganizationColumns =
            ["vat_code_id", "organization_id"];

        private static readonly string[] OrganizationVatCodeColumns =
            ["organization_id", "vat_code_id"];

        private static readonly string[] OrganizationInvoiceLineColumns =
            ["organization_id", "sales_invoice_id", "line_number"];

        private static readonly string[] OrganizationCustomerDateColumns =
            ["organization_id", "counterparty_id", "invoice_date"];

        private static readonly string[] OrganizationDateColumns =
            ["organization_id", "invoice_date"];

        private static readonly string[] OrganizationStatusDateColumns =
            ["organization_id", "status", "invoice_date"];

        private static readonly string[] OrganizationNumberColumns =
            ["organization_id", "number"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.CreateTable(
                name: "invoices",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    counterparty_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_date = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    base_currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    exchange_rate = table.Column<decimal>(type: "numeric(22,10)", precision: 22, scale: 10, nullable: true),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    customer_registration_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    customer_vat_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    customer_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    customer_address_line_1 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    customer_address_line_2 = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    customer_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    customer_postal_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    net_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    vat_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    gross_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    issued_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_invoices", x => x.id);
                    table.UniqueConstraint("ak_sales_invoices_id_organization", x => new { x.id, x.organization_id });
                    table.CheckConstraint("ck_sales_invoices_dates", "due_date >= invoice_date");
                    table.CheckConstraint("ck_sales_invoices_exchange_rate", "exchange_rate IS NULL OR exchange_rate > 0");
                    table.CheckConstraint("ck_sales_invoices_lifecycle", "(status = 'Draft' AND number IS NULL AND issued_at_utc IS NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL) OR (status = 'Issued' AND number IS NOT NULL AND issued_at_utc IS NOT NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL) OR (status = 'Cancelled' AND number IS NOT NULL AND issued_at_utc IS NOT NULL AND cancelled_at_utc IS NOT NULL AND cancellation_reason IS NOT NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL)");
                    table.CheckConstraint("ck_sales_invoices_totals", "net_total >= 0 AND vat_total >= 0 AND gross_total >= 0 AND net_total + vat_total = gross_total");
                    table.ForeignKey(
                        name: "FK_invoices_counterparties_counterparty_id_organization_id",
                        columns: x => new { x.counterparty_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "counterparties",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_currencies_base_currency_id_organization_id",
                        columns: x => new { x.base_currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_currencies_currency_id_organization_id",
                        columns: x => new { x.currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_invoice_number_sequences",
                schema: "sales",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    calendar_year = table.Column<int>(type: "integer", nullable: false),
                    last_number = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_invoice_number_sequences", x => new { x.organization_id, x.calendar_year });
                    table.CheckConstraint("ck_sales_invoice_sequences_last_number", "last_number >= 0");
                    table.CheckConstraint("ck_sales_invoice_sequences_year", "calendar_year >= 1");
                    table.ForeignKey(
                        name: "FK_sales_invoice_number_sequences_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "core",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_lines",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    net_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    gross_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_lines", x => x.id);
                    table.CheckConstraint("ck_sales_invoice_lines_amounts", "net_amount >= 0 AND vat_amount >= 0 AND gross_amount >= 0 AND net_amount + vat_amount = gross_amount");
                    table.CheckConstraint("ck_sales_invoice_lines_discount", "discount_percent >= 0 AND discount_percent <= 100");
                    table.CheckConstraint("ck_sales_invoice_lines_quantity", "quantity > 0");
                    table.CheckConstraint("ck_sales_invoice_lines_unit_price", "unit_price >= 0");
                    table.CheckConstraint("ck_sales_invoice_lines_vat_rate", "vat_rate_percent >= 0 AND vat_rate_percent <= 100");
                    table.ForeignKey(
                        name: "FK_invoice_lines_invoices_sales_invoice_id_organization_id",
                        columns: x => new { x.sales_invoice_id, x.organization_id },
                        principalSchema: "sales",
                        principalTable: "invoices",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_lines_vat_codes_vat_code_id_organization_id",
                        columns: x => new { x.vat_code_id, x.organization_id },
                        principalSchema: "tax",
                        principalTable: "vat_codes",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_sales_invoice_id_organization_id",
                schema: "sales",
                table: "invoice_lines",
                columns: InvoiceOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_vat_code_id_organization_id",
                schema: "sales",
                table: "invoice_lines",
                columns: VatCodeOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoice_lines_organization_vat_code",
                schema: "sales",
                table: "invoice_lines",
                columns: OrganizationVatCodeColumns);

            migrationBuilder.CreateIndex(
                name: "ux_sales_invoice_lines_invoice_line_number",
                schema: "sales",
                table: "invoice_lines",
                columns: OrganizationInvoiceLineColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_base_currency_id_organization_id",
                schema: "sales",
                table: "invoices",
                columns: BaseCurrencyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_counterparty_id_organization_id",
                schema: "sales",
                table: "invoices",
                columns: CounterpartyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_currency_id_organization_id",
                schema: "sales",
                table: "invoices",
                columns: CurrencyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoices_organization_customer_date",
                schema: "sales",
                table: "invoices",
                columns: OrganizationCustomerDateColumns);

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoices_organization_date",
                schema: "sales",
                table: "invoices",
                columns: OrganizationDateColumns);

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoices_organization_status_date",
                schema: "sales",
                table: "invoices",
                columns: OrganizationStatusDateColumns);

            migrationBuilder.CreateIndex(
                name: "ux_sales_invoices_organization_number",
                schema: "sales",
                table: "invoices",
                columns: OrganizationNumberColumns,
                unique: true,
                filter: "number IS NOT NULL");

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION sales.enforce_invoice_immutability()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.status IN ('Issued', 'Cancelled') THEN
                            RAISE EXCEPTION 'Issued or cancelled sales invoices are immutable';
                        END IF;

                        RETURN OLD;
                    END IF;

                    IF NEW.id <> OLD.id
                       OR NEW.organization_id <> OLD.organization_id THEN
                        RAISE EXCEPTION 'Sales invoice identity and organization are immutable';
                    END IF;

                    IF OLD.status = 'Draft' THEN
                        IF NEW.status IN ('Draft', 'Issued') THEN
                            RETURN NEW;
                        END IF;

                        RAISE EXCEPTION 'Invalid sales invoice state transition from Draft';
                    END IF;

                    IF OLD.status = 'Issued' THEN
                        IF NEW.status = 'Cancelled'
                           AND (
                               to_jsonb(NEW)
                               - 'status'
                               - 'cancelled_at_utc'
                               - 'cancellation_reason'
                               - 'updated_at_utc'
                               - 'updated_by_user_id'
                           ) = (
                               to_jsonb(OLD)
                               - 'status'
                               - 'cancelled_at_utc'
                               - 'cancellation_reason'
                               - 'updated_at_utc'
                               - 'updated_by_user_id'
                           ) THEN
                            RETURN NEW;
                        END IF;

                        RAISE EXCEPTION 'Issued sales invoices are immutable';
                    END IF;

                    IF OLD.status = 'Cancelled' THEN
                        RAISE EXCEPTION 'Cancelled sales invoices are immutable';
                    END IF;

                    RETURN NEW;
                END;
                $$;

                CREATE TRIGGER trg_sales_invoices_immutability
                BEFORE UPDATE OR DELETE ON sales.invoices
                FOR EACH ROW
                EXECUTE FUNCTION sales.enforce_invoice_immutability();

                CREATE OR REPLACE FUNCTION sales.enforce_invoice_line_parent_draft()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    parent_status text;
                BEGIN
                    IF TG_OP = 'INSERT' THEN
                        SELECT status
                        INTO parent_status
                        FROM sales.invoices
                        WHERE id = NEW.sales_invoice_id
                          AND organization_id = NEW.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Sales invoice lines can only be inserted under draft invoices';
                        END IF;

                        RETURN NEW;
                    END IF;

                    IF TG_OP = 'UPDATE' THEN
                        SELECT status
                        INTO parent_status
                        FROM sales.invoices
                        WHERE id = OLD.sales_invoice_id
                          AND organization_id = OLD.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Lines under issued or cancelled sales invoices are immutable';
                        END IF;

                        SELECT status
                        INTO parent_status
                        FROM sales.invoices
                        WHERE id = NEW.sales_invoice_id
                          AND organization_id = NEW.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Sales invoice lines can only be moved to draft invoices';
                        END IF;

                        RETURN NEW;
                    END IF;

                    SELECT status
                    INTO parent_status
                    FROM sales.invoices
                    WHERE id = OLD.sales_invoice_id
                      AND organization_id = OLD.organization_id;

                    IF parent_status IS DISTINCT FROM 'Draft' THEN
                        RAISE EXCEPTION 'Lines under issued or cancelled sales invoices are immutable';
                    END IF;

                    RETURN OLD;
                END;
                $$;

                CREATE TRIGGER trg_sales_invoice_lines_parent_draft
                BEFORE INSERT OR UPDATE OR DELETE ON sales.invoice_lines
                FOR EACH ROW
                EXECUTE FUNCTION sales.enforce_invoice_line_parent_draft();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_sales_invoice_lines_parent_draft
                    ON sales.invoice_lines;

                DROP FUNCTION IF EXISTS sales.enforce_invoice_line_parent_draft();

                DROP TRIGGER IF EXISTS trg_sales_invoices_immutability
                    ON sales.invoices;

                DROP FUNCTION IF EXISTS sales.enforce_invoice_immutability();
                """);

            migrationBuilder.DropTable(
                name: "invoice_lines",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "sales_invoice_number_sequences",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "invoices",
                schema: "sales");
        }
    }
}
