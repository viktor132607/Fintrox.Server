using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class FiscalYearConfiguration : IEntityTypeConfiguration<FiscalYear>
{
    public void Configure(EntityTypeBuilder<FiscalYear> builder)
    {
        builder.ToTable(
            "fiscal_years",
            DatabaseSchemas.Accounting,
            table => table.HasCheckConstraint(
                "ck_fiscal_years_date_range",
                ""end_date" >= "start_date""));

        builder.HasKey(year => year.Id);

        builder.HasAlternateKey(year => new
            {
                year.Id,
                year.OrganizationId
            })
            .HasName("ak_fiscal_years_id_organization");

        builder.Property(year => year.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(year => year.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(year => year.Name)
            .HasColumnName("name")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(year => year.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(year => year.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(year => year.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(year => year.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(year => year.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(year => year.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(year => year.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(year => new
            {
                year.OrganizationId,
                year.Name
            })
            .IsUnique()
            .HasDatabaseName("ux_fiscal_years_organization_name");

        builder.HasIndex(year => new
            {
                year.OrganizationId,
                year.StartDate,
                year.EndDate
            })
            .HasDatabaseName("ix_fiscal_years_organization_range");

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(year => year.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
