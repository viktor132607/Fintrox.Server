using Fintrox.Domain.Partners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class CounterpartyConfiguration
    : IEntityTypeConfiguration<Counterparty>
{
    public void Configure(EntityTypeBuilder<Counterparty> builder)
    {
        builder.ToTable(
            "counterparties",
            DatabaseSchemas.Core,
            table => table.HasCheckConstraint(
                "ck_counterparties_role",
                "is_customer OR is_supplier"));

        builder.HasKey(counterparty => counterparty.Id);

        builder.HasAlternateKey(counterparty => new
            {
                counterparty.Id,
                counterparty.OrganizationId
            })
            .HasName("ak_counterparties_id_organization");

        builder.Property(counterparty => counterparty.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(counterparty => counterparty.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(counterparty => counterparty.Code)
            .HasColumnName("code")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(counterparty => counterparty.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(counterparty => counterparty.LegalName)
            .HasColumnName("legal_name")
            .HasMaxLength(200);

        builder.Property(counterparty => counterparty.CountryCode)
            .HasColumnName("country_code")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(counterparty => counterparty.RegistrationNumber)
            .HasColumnName("registration_number")
            .HasMaxLength(64);

        builder.Property(counterparty => counterparty.VatNumber)
            .HasColumnName("vat_number")
            .HasMaxLength(64);

        builder.Property(counterparty => counterparty.IsCustomer)
            .HasColumnName("is_customer")
            .IsRequired();

        builder.Property(counterparty => counterparty.IsSupplier)
            .HasColumnName("is_supplier")
            .IsRequired();

        builder.Property(counterparty => counterparty.PaymentTermDays)
            .HasColumnName("payment_term_days")
            .IsRequired();

        builder.Property(counterparty => counterparty.ContactPerson)
            .HasColumnName("contact_person")
            .HasMaxLength(160);

        builder.Property(counterparty => counterparty.Email)
            .HasColumnName("email")
            .HasMaxLength(254);

        builder.Property(counterparty => counterparty.Phone)
            .HasColumnName("phone")
            .HasMaxLength(64);

        builder.Property(counterparty => counterparty.AddressLine1)
            .HasColumnName("address_line_1")
            .HasMaxLength(240);

        builder.Property(counterparty => counterparty.AddressLine2)
            .HasColumnName("address_line_2")
            .HasMaxLength(240);

        builder.Property(counterparty => counterparty.City)
            .HasColumnName("city")
            .HasMaxLength(120);

        builder.Property(counterparty => counterparty.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(32);

        builder.Property(counterparty => counterparty.Website)
            .HasColumnName("website")
            .HasMaxLength(255);

        builder.Property(counterparty => counterparty.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(counterparty => counterparty.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(counterparty => counterparty.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(counterparty => counterparty.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(counterparty => counterparty.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(counterparty => counterparty.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(counterparty => new
            {
                counterparty.OrganizationId,
                counterparty.Code
            })
            .IsUnique()
            .HasDatabaseName("ux_counterparties_organization_code");

        builder.HasIndex(counterparty => new
            {
                counterparty.OrganizationId,
                counterparty.CountryCode,
                counterparty.RegistrationNumber
            })
            .IsUnique()
            .HasFilter("registration_number IS NOT NULL")
            .HasDatabaseName(
                "ux_counterparties_organization_country_registration");

        builder.HasIndex(counterparty => new
            {
                counterparty.OrganizationId,
                counterparty.CountryCode,
                counterparty.VatNumber
            })
            .IsUnique()
            .HasFilter("vat_number IS NOT NULL")
            .HasDatabaseName(
                "ux_counterparties_organization_country_vat");

        builder.HasIndex(counterparty => new
            {
                counterparty.OrganizationId,
                counterparty.Name
            })
            .HasDatabaseName("ix_counterparties_organization_name");

        builder.HasIndex(counterparty => new
            {
                counterparty.OrganizationId,
                counterparty.IsActive,
                counterparty.IsCustomer,
                counterparty.IsSupplier
            })
            .HasDatabaseName(
                "ix_counterparties_organization_roles_active");

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(counterparty => counterparty.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
