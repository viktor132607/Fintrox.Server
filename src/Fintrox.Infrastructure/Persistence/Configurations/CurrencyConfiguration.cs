using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class CurrencyConfiguration
    : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable(
            "currencies",
            DatabaseSchemas.Core,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_currencies_decimal_places",
                    "decimal_places >= 0 AND decimal_places <= 4");

                table.HasCheckConstraint(
                    "ck_currencies_base_active",
                    "NOT is_base_currency OR is_active");
            });

        builder.HasKey(currency => currency.Id);

        builder.HasAlternateKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .HasName("ak_currencies_id_organization");

        builder.Property(currency => currency.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(currency => currency.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(currency => currency.Code)
            .HasColumnName("code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(currency => currency.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(currency => currency.Symbol)
            .HasColumnName("symbol")
            .HasMaxLength(12);

        builder.Property(currency => currency.DecimalPlaces)
            .HasColumnName("decimal_places")
            .IsRequired();

        builder.Property(currency => currency.IsBaseCurrency)
            .HasColumnName("is_base_currency")
            .IsRequired();

        builder.Property(currency => currency.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(currency => currency.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(currency => currency.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(currency => currency.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(currency => currency.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(currency => new
            {
                currency.OrganizationId,
                currency.Code
            })
            .IsUnique()
            .HasDatabaseName("ux_currencies_organization_code");

        builder.HasIndex(currency => currency.OrganizationId)
            .IsUnique()
            .HasFilter("is_base_currency")
            .HasDatabaseName("ux_currencies_organization_base");

        builder.HasIndex(currency => new
            {
                currency.OrganizationId,
                currency.IsActive
            })
            .HasDatabaseName("ix_currencies_organization_active");

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(currency => currency.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
