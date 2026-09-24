using Fintrox.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations", DatabaseSchemas.Core);

        builder.HasKey(organization => organization.Id);

        builder.Property(organization => organization.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(organization => organization.Name)
            .HasColumnName("name")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(organization => organization.LegalName)
            .HasColumnName("legal_name")
            .HasMaxLength(200);

        builder.Property(organization => organization.Slug)
            .HasColumnName("slug")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(organization => organization.CountryCode)
            .HasColumnName("country_code")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(organization => organization.BaseCurrencyCode)
            .HasColumnName("base_currency_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(organization => organization.TimeZoneId)
            .HasColumnName("time_zone_id")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(organization => organization.RegistrationNumber)
            .HasColumnName("registration_number")
            .HasMaxLength(64);

        builder.Property(organization => organization.VatNumber)
            .HasColumnName("vat_number")
            .HasMaxLength(64);

        builder.Property(organization => organization.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(organization => organization.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(organization => organization.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(organization => organization.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(organization => organization.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(organization => organization.Slug)
            .IsUnique()
            .HasDatabaseName("ux_organizations_slug");

        builder.HasIndex(organization => new
            {
                organization.CountryCode,
                organization.RegistrationNumber
            })
            .IsUnique()
            .HasDatabaseName("ux_organizations_country_registration_number");

        builder.HasIndex(organization => new
            {
                organization.CountryCode,
                organization.VatNumber
            })
            .IsUnique()
            .HasDatabaseName("ux_organizations_country_vat_number");
    }
}
