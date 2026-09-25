using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsAndAllocations : Migration
    {
        private static readonly string[] IdOrganizationColumns =
            ["id", "organization_id"];

        private static readonly string[] CounterpartyOrganizationColumns =
            ["counterparty_id", "organization_id"];

        private static readonly string[] CurrencyOrganizationColumns =
            ["currency_id", "organization_id"];

        private static readonly string[] BaseCurrencyOrganizationColumns =
            ["base_currency_id", "organization_id"];

        private static readonly string[] DocumentCurrencyOrganizationColumns =
            ["document_currency_id", "organization_id"];

        private static readonly string[] PurchaseDocumentOrganizationColumns =
            ["purchase_document_id", "organization_id"];

        private static readonly string[] SalesInvoiceOrganizationColumns =
            ["sales_invoice_id", "organization_id"];

        private static readonly string[] PaymentOrganizationColumns =
            ["payment_id", "organization_id"];

        private static readonly string[] OrganizationPurchaseDocumentColumns =
            ["organization_id", "purchase_document_id"];

        private static readonly string[] OrganizationSalesInvoiceColumns =
            ["organization_id", "sales_invoice_id"];

        private static readonly string[] OrganizationPaymentLineColumns =
            ["organization_id", "payment_id", "line_number"];

        private static readonly string[] OrganizationPaymentPurchaseColumns =
            ["organization_id", "payment_id", "purchase_document_id"];

        private static readonly string[] OrganizationPaymentSalesColumns =
            ["organization_id", "payment_id", "sales_invoice_id"];

        private static readonly string[] OrganizationCounterpartyDateColumns =
            ["organization_id", "counterparty_id", "payment_date"];

        private static readonly string[] OrganizationDateColumns =
            ["organization_id", "payment_date"];

        private static readonly string[] OrganizationStatusDateColumns =
            ["organization_id", "status", "payment_date"];

        private static readonly string[] OrganizationInternalNumberColumns =
            ["organization_id", "internal_number"];

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
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_currencies_base_currency_id_organization_id",
                        columns: x => new { x.base_currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_currencies_currency_id_organization_id",
                        columns: x => new { x.currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: IdOrganizationColumns,
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
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_allocations_documents_purchase_document_id_organization_id",
                        columns: x => new { x.purchase_document_id, x.organization_id },
                        principalSchema: "purchases",
                        principalTable: "documents",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_allocations_invoices_sales_invoice_id_organization_id",
                        columns: x => new { x.sales_invoice_id, x.organization_id },
                        principalSchema: "sales",
                        principalTable: "invoices",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_allocations_payments_payment_id_organization_id",
                        columns: x => new { x.payment_id, x.organization_id },
                        principalSchema: "payments",
                        principalTable: "payments",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_allocations_document_currency_id_organization_id",
                schema: "payments",
                table: "allocations",
                columns: DocumentCurrencyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_allocations_payment_id_organization_id",
                schema: "payments",
                table: "allocations",
                columns: PaymentOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_allocations_purchase_document_id_organization_id",
                schema: "payments",
                table: "allocations",
                columns: PurchaseDocumentOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_allocations_sales_invoice_id_organization_id",
                schema: "payments",
                table: "allocations",
                columns: SalesInvoiceOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_payment_allocations_purchase_document",
                schema: "payments",
                table: "allocations",
                columns: OrganizationPurchaseDocumentColumns);

            migrationBuilder.CreateIndex(
                name: "ix_payment_allocations_sales_invoice",
                schema: "payments",
                table: "allocations",
                columns: OrganizationSalesInvoiceColumns);

            migrationBuilder.CreateIndex(
                name: "ux_payment_allocations_payment_line_number",
                schema: "payments",
                table: "allocations",
                columns: OrganizationPaymentLineColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_payment_allocations_payment_purchase_document",
                schema: "payments",
                table: "allocations",
                columns: OrganizationPaymentPurchaseColumns,
                unique: true,
                filter: "purchase_document_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_payment_allocations_payment_sales_invoice",
                schema: "payments",
                table: "allocations",
                columns: OrganizationPaymentSalesColumns,
                unique: true,
                filter: "sales_invoice_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payments_base_currency_id_organization_id",
                schema: "payments",
                table: "payments",
                columns: BaseCurrencyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_payments_counterparty_id_organization_id",
                schema: "payments",
                table: "payments",
                columns: CounterpartyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_payments_currency_id_organization_id",
                schema: "payments",
                table: "payments",
                columns: CurrencyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_payments_organization_counterparty_date",
                schema: "payments",
                table: "payments",
                columns: OrganizationCounterpartyDateColumns);

            migrationBuilder.CreateIndex(
                name: "ix_payments_organization_date",
                schema: "payments",
                table: "payments",
                columns: OrganizationDateColumns);

            migrationBuilder.CreateIndex(
                name: "ix_payments_organization_status_date",
                schema: "payments",
                table: "payments",
                columns: OrganizationStatusDateColumns);

            migrationBuilder.CreateIndex(
                name: "ux_payments_organization_internal_number",
                schema: "payments",
                table: "payments",
                columns: OrganizationInternalNumberColumns,
                unique: true,
                filter: "internal_number IS NOT NULL");

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION payments.enforce_payment_immutability()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $payment$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.status IN ('Confirmed', 'Cancelled') THEN
                            RAISE EXCEPTION 'Confirmed or cancelled payments are immutable';
                        END IF;

                        RETURN OLD;
                    END IF;

                    IF NEW.id <> OLD.id
                       OR NEW.organization_id <> OLD.organization_id THEN
                        RAISE EXCEPTION 'Payment identity and organization are immutable';
                    END IF;

                    IF OLD.status = 'Draft' THEN
                        IF NEW.status IN ('Draft', 'Confirmed') THEN
                            RETURN NEW;
                        END IF;

                        RAISE EXCEPTION 'Invalid payment state transition from Draft';
                    END IF;

                    IF OLD.status = 'Confirmed' THEN
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

                        RAISE EXCEPTION 'Confirmed payments are immutable';
                    END IF;

                    IF OLD.status = 'Cancelled' THEN
                        RAISE EXCEPTION 'Cancelled payments are immutable';
                    END IF;

                    RETURN NEW;
                END;
                $payment$;

                CREATE TRIGGER trg_payments_immutability
                BEFORE UPDATE OR DELETE ON payments.payments
                FOR EACH ROW
                EXECUTE FUNCTION payments.enforce_payment_immutability();

                CREATE OR REPLACE FUNCTION payments.enforce_allocation_parent_draft()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $allocation$
                DECLARE
                    parent_status text;
                BEGIN
                    IF TG_OP = 'INSERT' THEN
                        SELECT status
                        INTO parent_status
                        FROM payments.payments
                        WHERE id = NEW.payment_id
                          AND organization_id = NEW.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Allocations can only be inserted under draft payments';
                        END IF;

                        RETURN NEW;
                    END IF;

                    IF TG_OP = 'UPDATE' THEN
                        SELECT status
                        INTO parent_status
                        FROM payments.payments
                        WHERE id = OLD.payment_id
                          AND organization_id = OLD.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Allocations under confirmed or cancelled payments are immutable';
                        END IF;

                        SELECT status
                        INTO parent_status
                        FROM payments.payments
                        WHERE id = NEW.payment_id
                          AND organization_id = NEW.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Allocations can only be moved to draft payments';
                        END IF;

                        RETURN NEW;
                    END IF;

                    SELECT status
                    INTO parent_status
                    FROM payments.payments
                    WHERE id = OLD.payment_id
                      AND organization_id = OLD.organization_id;

                    IF parent_status IS DISTINCT FROM 'Draft' THEN
                        RAISE EXCEPTION 'Allocations under confirmed or cancelled payments are immutable';
                    END IF;

                    RETURN OLD;
                END;
                $allocation$;

                CREATE TRIGGER trg_payment_allocations_parent_draft
                BEFORE INSERT OR UPDATE OR DELETE ON payments.allocations
                FOR EACH ROW
                EXECUTE FUNCTION payments.enforce_allocation_parent_draft();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_payment_allocations_parent_draft
                    ON payments.allocations;

                DROP FUNCTION IF EXISTS payments.enforce_allocation_parent_draft();

                DROP TRIGGER IF EXISTS trg_payments_immutability
                    ON payments.payments;

                DROP FUNCTION IF EXISTS payments.enforce_payment_immutability();
                """);

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
