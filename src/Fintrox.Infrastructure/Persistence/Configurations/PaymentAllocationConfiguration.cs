using Fintrox.Domain.Accounting;
using Fintrox.Domain.Payments;
using Fintrox.Domain.Purchases;
using Fintrox.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class PaymentAllocationConfiguration
    : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable(
            "allocations",
            DatabaseSchemas.Payments,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_payment_allocations_target",
                    "(target_type = 'SalesInvoice' AND sales_invoice_id IS NOT NULL AND purchase_document_id IS NULL) OR " +
                    "(target_type = 'PurchaseDocument' AND purchase_document_id IS NOT NULL AND sales_invoice_id IS NULL)");

                table.HasCheckConstraint(
                    "ck_payment_allocations_exchange_rate",
                    "document_exchange_rate > 0");

                table.HasCheckConstraint(
                    "ck_payment_allocations_amounts",
                    "document_amount > 0 AND payment_amount > 0");
            });

        builder.HasKey(allocation => allocation.Id);

        builder.Property(allocation => allocation.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(allocation => allocation.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(allocation => allocation.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(allocation => allocation.LineNumber)
            .HasColumnName("line_number")
            .IsRequired();

        builder.Property(allocation => allocation.TargetType)
            .HasColumnName("target_type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(allocation => allocation.SalesInvoiceId)
            .HasColumnName("sales_invoice_id");

        builder.Property(allocation => allocation.PurchaseDocumentId)
            .HasColumnName("purchase_document_id");

        builder.Property(allocation => allocation.DocumentNumber)
            .HasColumnName("document_number")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(allocation => allocation.DocumentCurrencyId)
            .HasColumnName("document_currency_id")
            .IsRequired();

        builder.Property(allocation => allocation.DocumentCurrencyCode)
            .HasColumnName("document_currency_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(allocation => allocation.DocumentExchangeRate)
            .HasColumnName("document_exchange_rate")
            .HasPrecision(22, 10)
            .IsRequired();

        builder.Property(allocation => allocation.DocumentAmount)
            .HasColumnName("document_amount")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(allocation => allocation.PaymentAmount)
            .HasColumnName("payment_amount")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(allocation => allocation.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(allocation => allocation.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(allocation => allocation.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(allocation => allocation.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(allocation => new
            {
                allocation.OrganizationId,
                allocation.PaymentId,
                allocation.LineNumber
            })
            .IsUnique()
            .HasDatabaseName("ux_payment_allocations_payment_line_number");

        builder.HasIndex(allocation => new
            {
                allocation.OrganizationId,
                allocation.PaymentId,
                allocation.SalesInvoiceId
            })
            .IsUnique()
            .HasFilter("sales_invoice_id IS NOT NULL")
            .HasDatabaseName("ux_payment_allocations_payment_sales_invoice");

        builder.HasIndex(allocation => new
            {
                allocation.OrganizationId,
                allocation.PaymentId,
                allocation.PurchaseDocumentId
            })
            .IsUnique()
            .HasFilter("purchase_document_id IS NOT NULL")
            .HasDatabaseName("ux_payment_allocations_payment_purchase_document");

        builder.HasIndex(allocation => new
            {
                allocation.OrganizationId,
                allocation.SalesInvoiceId
            })
            .HasDatabaseName("ix_payment_allocations_sales_invoice");

        builder.HasIndex(allocation => new
            {
                allocation.OrganizationId,
                allocation.PurchaseDocumentId
            })
            .HasDatabaseName("ix_payment_allocations_purchase_document");

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(allocation => new
            {
                allocation.PaymentId,
                allocation.OrganizationId
            })
            .HasPrincipalKey(payment => new
            {
                payment.Id,
                payment.OrganizationId
            })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<SalesInvoice>()
            .WithMany()
            .HasForeignKey(allocation => new
            {
                allocation.SalesInvoiceId,
                allocation.OrganizationId
            })
            .HasPrincipalKey(invoice => new
            {
                invoice.Id,
                invoice.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PurchaseDocument>()
            .WithMany()
            .HasForeignKey(allocation => new
            {
                allocation.PurchaseDocumentId,
                allocation.OrganizationId
            })
            .HasPrincipalKey(document => new
            {
                document.Id,
                document.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(allocation => new
            {
                allocation.DocumentCurrencyId,
                allocation.OrganizationId
            })
            .HasPrincipalKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
