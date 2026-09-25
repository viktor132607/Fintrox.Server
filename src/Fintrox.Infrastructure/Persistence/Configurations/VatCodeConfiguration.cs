using Fintrox.Domain.Tax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class VatCodeConfiguration
    : IEntityTypeConfiguration<VatCode>
{
    public void Configure(EntityTypeBuilder<VatCode> builder)
    {
        builder.ToTable(
            "vat_codes",
            DatabaseSchemas.Tax,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_vat_codes_rate",
                    "rate_percent >= 0 AND rate_percent <= 100");

                table.HasCheckConstraint(
                    "ck_vat_codes_validity",
                    "valid_to IS NULL OR valid_to >= valid_from");

                table.HasCheckConstraint(
                    "ck_vat_codes_applicability",
                    "applies_to_sales OR applies_to_purchases");

                table.HasCheckConstraint(
                    "ck_vat_codes_kind_rate",
                    "(kind IN ('ZeroRated', 'Exempt', 'OutOfScope') AND rate_percent = 0) OR " +
                    "(kind IN ('Standard', 'Reduced') AND rate_percent > 0) OR " +
                    "kind = 'ReverseCharge'");
            });

        builder.HasKey(vatCode => vatCode.Id);

        builder.HasAlternateKey(vatCode => new
            {
                vatCode.Id,
                vatCode.OrganizationId
            })
            .HasName("ak_vat_codes_id_organization");

        builder.Property(vatCode => vatCode.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(vatCode => vatCode.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(vatCode => vatCode.Code)
            .HasColumnName("code")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(vatCode => vatCode.Name)
            .HasColumnName("name")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(vatCode => vatCode.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(vatCode => vatCode.RatePercent)
            .HasColumnName("rate_percent")
            .HasPrecision(7, 4)
            .IsRequired();

        builder.Property(vatCode => vatCode.ValidFrom)
            .HasColumnName("valid_from")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(vatCode => vatCode.ValidTo)
            .HasColumnName("valid_to")
            .HasColumnType("date");

        builder.Property(vatCode => vatCode.AppliesToSales)
            .HasColumnName("applies_to_sales")
            .IsRequired();

        builder.Property(vatCode => vatCode.AppliesToPurchases)
            .HasColumnName("applies_to_purchases")
            .IsRequired();

        builder.Property(vatCode => vatCode.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(vatCode => vatCode.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(vatCode => vatCode.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(vatCode => vatCode.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(vatCode => vatCode.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(vatCode => new
            {
                vatCode.OrganizationId,
                vatCode.Code,
                vatCode.ValidFrom
            })
            .IsUnique()
            .HasDatabaseName("ux_vat_codes_organization_code_valid_from");

        builder.HasIndex(vatCode => new
            {
                vatCode.OrganizationId,
                vatCode.IsActive,
                vatCode.ValidFrom,
                vatCode.ValidTo
            })
            .HasDatabaseName("ix_vat_codes_organization_validity");

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(vatCode => vatCode.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
