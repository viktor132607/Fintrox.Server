using Fintrox.Domain.Accounting;
using Fintrox.Domain.Partners;
using Fintrox.Domain.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class PurchaseDocumentConfiguration
    : IEntityTypeConfiguration<PurchaseDocument>
{
    public void Configure(EntityTypeBuilder<PurchaseDocument> builder)
    {
        builder.ToTable(
            "documents",
            DatabaseSchemas.Purchases,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_purchase_documents_dates",
                    "due_date >= document_date");

                table.HasCheckConstraint(
                    "ck_purchase_documents_totals",
                    "net_total >= 0 AND vat_total >= 0 AND recoverable_vat_total >= 0 AND non_recoverable_vat_total >= 0 AND gross_total >= 0 AND net_total + vat_total = gross_total AND recoverable_vat_total + non_recoverable_vat_total = vat_total");

                table.HasCheckConstraint(
                    "ck_purchase_documents_exchange_rate",
                    "exchange_rate IS NULL OR exchange_rate > 0");

                table.HasCheckConstraint(
                    "ck_purchase_documents_invoice_supplier_number",
                    "type <> 'Invoice' OR status = 'Draft' OR supplier_document_number IS NOT NULL");

                table.HasCheckConstraint(
                    "ck_purchase_documents_lifecycle",
                    "(status = 'Draft' AND internal_number IS NULL AND received_at_utc IS NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL) OR " +
                    "(status = 'Received' AND internal_number IS NOT NULL AND received_at_utc IS NOT NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL) OR " +
                    "(status = 'Cancelled' AND internal_number IS NOT NULL AND received_at_utc IS NOT NULL AND cancelled_at_utc IS NOT NULL AND cancellation_reason IS NOT NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL)");
            });

        builder.HasKey(document => document.Id);

        builder.HasAlternateKey(document => new
            {
                document.Id,
                document.OrganizationId
            })
            .HasName("ak_purchase_documents_id_organization");

        builder.Property(document => document.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(document => document.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(document => document.InternalNumber)
            .HasColumnName("internal_number")
            .HasMaxLength(40);

        builder.Property(document => document.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(document => document.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(document => document.CounterpartyId)
            .HasColumnName("counterparty_id")
            .IsRequired();

        builder.Property(document => document.SupplierDocumentNumber)
            .HasColumnName("supplier_document_number")
            .HasMaxLength(80);

        builder.Property(document => document.DocumentDate)
            .HasColumnName("document_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(document => document.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(document => document.CurrencyId)
            .HasColumnName("currency_id")
            .IsRequired();

        builder.Property(document => document.BaseCurrencyId)
            .HasColumnName("base_currency_id");

        builder.Property(document => document.CurrencyCode)
            .HasColumnName("currency_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(document => document.BaseCurrencyCode)
            .HasColumnName("base_currency_code")
            .HasMaxLength(3);

        builder.Property(document => document.ExchangeRate)
            .HasColumnName("exchange_rate")
            .HasPrecision(22, 10);

        builder.Property(document => document.SupplierName)
            .HasColumnName("supplier_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(document => document.SupplierLegalName)
            .HasColumnName("supplier_legal_name")
            .HasMaxLength(200);

        builder.Property(document => document.SupplierRegistrationNumber)
            .HasColumnName("supplier_registration_number")
            .HasMaxLength(64);

        builder.Property(document => document.SupplierVatNumber)
            .HasColumnName("supplier_vat_number")
            .HasMaxLength(64);

        builder.Property(document => document.SupplierCountryCode)
            .HasColumnName("supplier_country_code")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(document => document.SupplierAddressLine1)
            .HasColumnName("supplier_address_line_1")
            .HasMaxLength(240);

        builder.Property(document => document.SupplierAddressLine2)
            .HasColumnName("supplier_address_line_2")
            .HasMaxLength(240);

        builder.Property(document => document.SupplierCity)
            .HasColumnName("supplier_city")
            .HasMaxLength(120);

        builder.Property(document => document.SupplierPostalCode)
            .HasColumnName("supplier_postal_code")
            .HasMaxLength(32);

        builder.Property(document => document.NetTotal)
            .HasColumnName("net_total")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(document => document.VatTotal)
            .HasColumnName("vat_total")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(document => document.RecoverableVatTotal)
            .HasColumnName("recoverable_vat_total")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(document => document.NonRecoverableVatTotal)
            .HasColumnName("non_recoverable_vat_total")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(document => document.GrossTotal)
            .HasColumnName("gross_total")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(document => document.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(document => document.ReceivedAtUtc)
            .HasColumnName("received_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(document => document.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(document => document.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(500);

        builder.Property(document => document.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(document => document.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(document => document.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(document => document.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(document => new
            {
                document.OrganizationId,
                document.InternalNumber
            })
            .IsUnique()
            .HasFilter("internal_number IS NOT NULL")
            .HasDatabaseName("ux_purchase_documents_organization_internal_number");

        builder.HasIndex(document => new
            {
                document.OrganizationId,
                document.CounterpartyId,
                document.SupplierDocumentNumber
            })
            .IsUnique()
            .HasFilter("supplier_document_number IS NOT NULL")
            .HasDatabaseName("ux_purchase_documents_supplier_document_number");

        builder.HasIndex(document => new
            {
                document.OrganizationId,
                document.DocumentDate
            })
            .HasDatabaseName("ix_purchase_documents_organization_date");

        builder.HasIndex(document => new
            {
                document.OrganizationId,
                document.Status,
                document.DocumentDate
            })
            .HasDatabaseName("ix_purchase_documents_organization_status_date");

        builder.HasIndex(document => new
            {
                document.OrganizationId,
                document.Type,
                document.DocumentDate
            })
            .HasDatabaseName("ix_purchase_documents_organization_type_date");

        builder.HasOne<Counterparty>()
            .WithMany()
            .HasForeignKey(document => new
            {
                document.CounterpartyId,
                document.OrganizationId
            })
            .HasPrincipalKey(counterparty => new
            {
                counterparty.Id,
                counterparty.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(document => new
            {
                document.CurrencyId,
                document.OrganizationId
            })
            .HasPrincipalKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(document => new
            {
                document.BaseCurrencyId,
                document.OrganizationId
            })
            .HasPrincipalKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(document => document.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
