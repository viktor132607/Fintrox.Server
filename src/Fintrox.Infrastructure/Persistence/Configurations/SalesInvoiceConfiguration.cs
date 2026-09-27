using Fintrox.Domain.Accounting;
using Fintrox.Domain.Partners;
using Fintrox.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class SalesInvoiceConfiguration
    : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        builder.ToTable(
            "invoices",
            DatabaseSchemas.Sales,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_sales_invoices_dates",
                    "due_date >= invoice_date");

                table.HasCheckConstraint(
                    "ck_sales_invoices_totals",
                    "net_total >= 0 AND vat_total >= 0 AND gross_total >= 0 AND net_total + vat_total = gross_total");

                table.HasCheckConstraint(
                    "ck_sales_invoices_exchange_rate",
                    "exchange_rate IS NULL OR exchange_rate > 0");

                table.HasCheckConstraint(
                    "ck_sales_invoices_lifecycle",
                    "(status = 'Draft' AND number IS NULL AND issued_at_utc IS NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL) OR " +
                    "(status = 'Issued' AND number IS NOT NULL AND issued_at_utc IS NOT NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL) OR " +
                    "(status = 'Cancelled' AND number IS NOT NULL AND issued_at_utc IS NOT NULL AND cancelled_at_utc IS NOT NULL AND cancellation_reason IS NOT NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL)");
            });

        builder.HasKey(invoice => invoice.Id);

        builder.HasAlternateKey(invoice => new
            {
                invoice.Id,
                invoice.OrganizationId
            })
            .HasName("ak_sales_invoices_id_organization");

        builder.Property(invoice => invoice.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(invoice => invoice.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(invoice => invoice.Number)
            .HasColumnName("number")
            .HasMaxLength(40);

        builder.Property(invoice => invoice.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(invoice => invoice.CounterpartyId)
            .HasColumnName("counterparty_id")
            .IsRequired();

        builder.Property(invoice => invoice.InvoiceDate)
            .HasColumnName("invoice_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(invoice => invoice.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(invoice => invoice.CurrencyId)
            .HasColumnName("currency_id")
            .IsRequired();

        builder.Property(invoice => invoice.BaseCurrencyId)
            .HasColumnName("base_currency_id");

        builder.Property(invoice => invoice.CurrencyCode)
            .HasColumnName("currency_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(invoice => invoice.BaseCurrencyCode)
            .HasColumnName("base_currency_code")
            .HasMaxLength(3);

        builder.Property(invoice => invoice.ExchangeRate)
            .HasColumnName("exchange_rate")
            .HasPrecision(22, 10);

        builder.Property(invoice => invoice.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(invoice => invoice.CustomerLegalName)
            .HasColumnName("customer_legal_name")
            .HasMaxLength(200);

        builder.Property(invoice => invoice.CustomerRegistrationNumber)
            .HasColumnName("customer_registration_number")
            .HasMaxLength(64);

        builder.Property(invoice => invoice.CustomerVatNumber)
            .HasColumnName("customer_vat_number")
            .HasMaxLength(64);

        builder.Property(invoice => invoice.CustomerCountryCode)
            .HasColumnName("customer_country_code")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(invoice => invoice.CustomerAddressLine1)
            .HasColumnName("customer_address_line_1")
            .HasMaxLength(240);

        builder.Property(invoice => invoice.CustomerAddressLine2)
            .HasColumnName("customer_address_line_2")
            .HasMaxLength(240);

        builder.Property(invoice => invoice.CustomerCity)
            .HasColumnName("customer_city")
            .HasMaxLength(120);

        builder.Property(invoice => invoice.CustomerPostalCode)
            .HasColumnName("customer_postal_code")
            .HasMaxLength(32);

        builder.Property(invoice => invoice.NetTotal)
            .HasColumnName("net_total")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(invoice => invoice.VatTotal)
            .HasColumnName("vat_total")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(invoice => invoice.GrossTotal)
            .HasColumnName("gross_total")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(invoice => invoice.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(invoice => invoice.IssuedAtUtc)
            .HasColumnName("issued_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(invoice => invoice.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(invoice => invoice.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(500);

        builder.Property(invoice => invoice.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(invoice => invoice.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(invoice => invoice.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(invoice => invoice.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(invoice => new
            {
                invoice.OrganizationId,
                invoice.Number
            })
            .IsUnique()
            .HasFilter("number IS NOT NULL")
            .HasDatabaseName("ux_sales_invoices_organization_number");

        builder.HasIndex(invoice => new
            {
                invoice.OrganizationId,
                invoice.InvoiceDate
            })
            .HasDatabaseName("ix_sales_invoices_organization_date");

        builder.HasIndex(invoice => new
            {
                invoice.OrganizationId,
                invoice.Status,
                invoice.InvoiceDate
            })
            .HasDatabaseName("ix_sales_invoices_organization_status_date");

        builder.HasIndex(invoice => new
            {
                invoice.OrganizationId,
                invoice.CounterpartyId,
                invoice.InvoiceDate
            })
            .HasDatabaseName("ix_sales_invoices_organization_customer_date");

        builder.HasOne<Counterparty>()
            .WithMany()
            .HasForeignKey(invoice => new
            {
                invoice.CounterpartyId,
                invoice.OrganizationId
            })
            .HasPrincipalKey(counterparty => new
            {
                counterparty.Id,
                counterparty.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(invoice => new
            {
                invoice.CurrencyId,
                invoice.OrganizationId
            })
            .HasPrincipalKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(invoice => new
            {
                invoice.BaseCurrencyId,
                invoice.OrganizationId
            })
            .HasPrincipalKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(invoice => invoice.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
