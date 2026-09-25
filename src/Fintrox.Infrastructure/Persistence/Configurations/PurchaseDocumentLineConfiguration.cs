using Fintrox.Domain.Purchases;
using Fintrox.Domain.Tax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class PurchaseDocumentLineConfiguration
    : IEntityTypeConfiguration<PurchaseDocumentLine>
{
    public void Configure(EntityTypeBuilder<PurchaseDocumentLine> builder)
    {
        builder.ToTable(
            "document_lines",
            DatabaseSchemas.Purchases,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_purchase_document_lines_quantity",
                    "quantity > 0");

                table.HasCheckConstraint(
                    "ck_purchase_document_lines_unit_price",
                    "unit_price >= 0");

                table.HasCheckConstraint(
                    "ck_purchase_document_lines_discount",
                    "discount_percent >= 0 AND discount_percent <= 100");

                table.HasCheckConstraint(
                    "ck_purchase_document_lines_vat_rate",
                    "vat_rate_percent >= 0 AND vat_rate_percent <= 100");

                table.HasCheckConstraint(
                    "ck_purchase_document_lines_recoverable_vat",
                    "recoverable_vat_percent >= 0 AND recoverable_vat_percent <= 100");

                table.HasCheckConstraint(
                    "ck_purchase_document_lines_amounts",
                    "net_amount >= 0 AND vat_amount >= 0 AND recoverable_vat_amount >= 0 AND non_recoverable_vat_amount >= 0 AND gross_amount >= 0 AND net_amount + vat_amount = gross_amount AND recoverable_vat_amount + non_recoverable_vat_amount = vat_amount");
            });

        builder.HasKey(line => line.Id);

        builder.Property(line => line.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(line => line.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(line => line.PurchaseDocumentId)
            .HasColumnName("purchase_document_id")
            .IsRequired();

        builder.Property(line => line.LineNumber)
            .HasColumnName("line_number")
            .IsRequired();

        builder.Property(line => line.ItemCode)
            .HasColumnName("item_code")
            .HasMaxLength(100);

        builder.Property(line => line.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(line => line.Quantity)
            .HasColumnName("quantity")
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(line => line.UnitOfMeasure)
            .HasColumnName("unit_of_measure")
            .HasMaxLength(32);

        builder.Property(line => line.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(line => line.DiscountPercent)
            .HasColumnName("discount_percent")
            .HasPrecision(7, 4)
            .IsRequired();

        builder.Property(line => line.VatCodeId)
            .HasColumnName("vat_code_id")
            .IsRequired();

        builder.Property(line => line.VatCode)
            .HasColumnName("vat_code")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(line => line.VatRatePercent)
            .HasColumnName("vat_rate_percent")
            .HasPrecision(7, 4)
            .IsRequired();

        builder.Property(line => line.RecoverableVatPercent)
            .HasColumnName("recoverable_vat_percent")
            .HasPrecision(7, 4)
            .IsRequired();

        builder.Property(line => line.NetAmount)
            .HasColumnName("net_amount")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(line => line.VatAmount)
            .HasColumnName("vat_amount")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(line => line.RecoverableVatAmount)
            .HasColumnName("recoverable_vat_amount")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(line => line.NonRecoverableVatAmount)
            .HasColumnName("non_recoverable_vat_amount")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(line => line.GrossAmount)
            .HasColumnName("gross_amount")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(line => line.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(line => line.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(line => line.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(line => line.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(line => new
            {
                line.OrganizationId,
                line.PurchaseDocumentId,
                line.LineNumber
            })
            .IsUnique()
            .HasDatabaseName("ux_purchase_document_lines_document_line_number");

        builder.HasIndex(line => new
            {
                line.OrganizationId,
                line.VatCodeId
            })
            .HasDatabaseName("ix_purchase_document_lines_organization_vat_code");

        builder.HasOne<PurchaseDocument>()
            .WithMany()
            .HasForeignKey(line => new
            {
                line.PurchaseDocumentId,
                line.OrganizationId
            })
            .HasPrincipalKey(document => new
            {
                document.Id,
                document.OrganizationId
            })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<VatCode>()
            .WithMany()
            .HasForeignKey(line => new
            {
                line.VatCodeId,
                line.OrganizationId
            })
            .HasPrincipalKey(vatCode => new
            {
                vatCode.Id,
                vatCode.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
