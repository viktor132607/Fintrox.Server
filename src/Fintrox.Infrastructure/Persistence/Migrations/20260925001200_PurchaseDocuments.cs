using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseDocuments : Migration
    {
        private static readonly string[] IdOrganizationColumns =
            ["id", "organization_id"];

        private static readonly string[] CounterpartyOrganizationColumns =
            ["counterparty_id", "organization_id"];

        private static readonly string[] CurrencyOrganizationColumns =
            ["currency_id", "organization_id"];

        private static readonly string[] BaseCurrencyOrganizationColumns =
            ["base_currency_id", "organization_id"];

        private static readonly string[] DocumentOrganizationColumns =
            ["purchase_document_id", "organization_id"];

        private static readonly string[] VatCodeOrganizationColumns =
            ["vat_code_id", "organization_id"];

        private static readonly string[] OrganizationVatCodeColumns =
            ["organization_id", "vat_code_id"];

        private static readonly string[] OrganizationDocumentLineColumns =
            ["organization_id", "purchase_document_id", "line_number"];

        private static readonly string[] OrganizationDateColumns =
            ["organization_id", "document_date"];

        private static readonly string[] OrganizationStatusDateColumns =
            ["organization_id", "status", "document_date"];

        private static readonly string[] OrganizationTypeDateColumns =
            ["organization_id", "type", "document_date"];

        private static readonly string[] OrganizationInternalNumberColumns =
            ["organization_id", "internal_number"];

        private static readonly string[] SupplierDocumentNumberColumns =
            ["organization_id", "counterparty_id", "supplier_document_number"];

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
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documents_currencies_base_currency_id_organization_id",
                        columns: x => new { x.base_currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documents_currencies_currency_id_organization_id",
                        columns: x => new { x.currency_id, x.organization_id },
                        principalSchema: "core",
                        principalTable: "currencies",
                        principalColumns: IdOrganizationColumns,
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
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_lines_vat_codes_vat_code_id_organization_id",
                        columns: x => new { x.vat_code_id, x.organization_id },
                        principalSchema: "tax",
                        principalTable: "vat_codes",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_lines_purchase_document_id_organization_id",
                schema: "purchases",
                table: "document_lines",
                columns: DocumentOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_document_lines_vat_code_id_organization_id",
                schema: "purchases",
                table: "document_lines",
                columns: VatCodeOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_document_lines_organization_vat_code",
                schema: "purchases",
                table: "document_lines",
                columns: OrganizationVatCodeColumns);

            migrationBuilder.CreateIndex(
                name: "ux_purchase_document_lines_document_line_number",
                schema: "purchases",
                table: "document_lines",
                columns: OrganizationDocumentLineColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_base_currency_id_organization_id",
                schema: "purchases",
                table: "documents",
                columns: BaseCurrencyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_documents_counterparty_id_organization_id",
                schema: "purchases",
                table: "documents",
                columns: CounterpartyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "IX_documents_currency_id_organization_id",
                schema: "purchases",
                table: "documents",
                columns: CurrencyOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_documents_organization_date",
                schema: "purchases",
                table: "documents",
                columns: OrganizationDateColumns);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_documents_organization_status_date",
                schema: "purchases",
                table: "documents",
                columns: OrganizationStatusDateColumns);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_documents_organization_type_date",
                schema: "purchases",
                table: "documents",
                columns: OrganizationTypeDateColumns);

            migrationBuilder.CreateIndex(
                name: "ux_purchase_documents_organization_internal_number",
                schema: "purchases",
                table: "documents",
                columns: OrganizationInternalNumberColumns,
                unique: true,
                filter: "internal_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_purchase_documents_supplier_document_number",
                schema: "purchases",
                table: "documents",
                columns: SupplierDocumentNumberColumns,
                unique: true,
                filter: "supplier_document_number IS NOT NULL");

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION purchases.enforce_document_immutability()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.status IN ('Received', 'Cancelled') THEN
                            RAISE EXCEPTION 'Received or cancelled purchase documents are immutable';
                        END IF;

                        RETURN OLD;
                    END IF;

                    IF NEW.id <> OLD.id
                       OR NEW.organization_id <> OLD.organization_id THEN
                        RAISE EXCEPTION 'Purchase document identity and organization are immutable';
                    END IF;

                    IF OLD.status = 'Draft' THEN
                        IF NEW.status IN ('Draft', 'Received') THEN
                            RETURN NEW;
                        END IF;

                        RAISE EXCEPTION 'Invalid purchase document state transition from Draft';
                    END IF;

                    IF OLD.status = 'Received' THEN
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

                        RAISE EXCEPTION 'Received purchase documents are immutable';
                    END IF;

                    IF OLD.status = 'Cancelled' THEN
                        RAISE EXCEPTION 'Cancelled purchase documents are immutable';
                    END IF;

                    RETURN NEW;
                END;
                $$;

                CREATE TRIGGER trg_purchase_documents_immutability
                BEFORE UPDATE OR DELETE ON purchases.documents
                FOR EACH ROW
                EXECUTE FUNCTION purchases.enforce_document_immutability();

                CREATE OR REPLACE FUNCTION purchases.enforce_document_line_parent_draft()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    parent_status text;
                BEGIN
                    IF TG_OP = 'INSERT' THEN
                        SELECT status
                        INTO parent_status
                        FROM purchases.documents
                        WHERE id = NEW.purchase_document_id
                          AND organization_id = NEW.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Purchase document lines can only be inserted under draft documents';
                        END IF;

                        RETURN NEW;
                    END IF;

                    IF TG_OP = 'UPDATE' THEN
                        SELECT status
                        INTO parent_status
                        FROM purchases.documents
                        WHERE id = OLD.purchase_document_id
                          AND organization_id = OLD.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Lines under received or cancelled purchase documents are immutable';
                        END IF;

                        SELECT status
                        INTO parent_status
                        FROM purchases.documents
                        WHERE id = NEW.purchase_document_id
                          AND organization_id = NEW.organization_id;

                        IF parent_status IS DISTINCT FROM 'Draft' THEN
                            RAISE EXCEPTION 'Purchase document lines can only be moved to draft documents';
                        END IF;

                        RETURN NEW;
                    END IF;

                    SELECT status
                    INTO parent_status
                    FROM purchases.documents
                    WHERE id = OLD.purchase_document_id
                      AND organization_id = OLD.organization_id;

                    IF parent_status IS DISTINCT FROM 'Draft' THEN
                        RAISE EXCEPTION 'Lines under received or cancelled purchase documents are immutable';
                    END IF;

                    RETURN OLD;
                END;
                $$;

                CREATE TRIGGER trg_purchase_document_lines_parent_draft
                BEFORE INSERT OR UPDATE OR DELETE ON purchases.document_lines
                FOR EACH ROW
                EXECUTE FUNCTION purchases.enforce_document_line_parent_draft();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_purchase_document_lines_parent_draft
                    ON purchases.document_lines;

                DROP FUNCTION IF EXISTS purchases.enforce_document_line_parent_draft();

                DROP TRIGGER IF EXISTS trg_purchase_documents_immutability
                    ON purchases.documents;

                DROP FUNCTION IF EXISTS purchases.enforce_document_immutability();
                """);

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
