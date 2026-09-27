using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class AccountingPeriodConfiguration
    : IEntityTypeConfiguration<AccountingPeriod>
{
    public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.ToTable(
            "accounting_periods",
            DatabaseSchemas.Accounting,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_accounting_periods_date_range",
                    "end_date >= start_date");

                table.HasCheckConstraint(
                    "ck_accounting_periods_number",
                    "number >= 1 AND number <= 99");
            });

        builder.HasKey(period => period.Id);

        builder.HasAlternateKey(period => new
            {
                period.Id,
                period.OrganizationId
            })
            .HasName("ak_accounting_periods_id_organization");

        builder.Property(period => period.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(period => period.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(period => period.FiscalYearId)
            .HasColumnName("fiscal_year_id")
            .IsRequired();

        builder.Property(period => period.Number)
            .HasColumnName("number")
            .IsRequired();

        builder.Property(period => period.Name)
            .HasColumnName("name")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(period => period.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(period => period.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(period => period.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(period => period.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(period => period.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(period => period.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(period => period.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(period => new
            {
                period.OrganizationId,
                period.FiscalYearId,
                period.Number
            })
            .IsUnique()
            .HasDatabaseName("ux_accounting_periods_year_number");

        builder.HasIndex(period => new
            {
                period.OrganizationId,
                period.FiscalYearId,
                period.StartDate,
                period.EndDate
            })
            .HasDatabaseName("ix_accounting_periods_year_range");

        builder.HasIndex(period => new
            {
                period.OrganizationId,
                period.Status
            })
            .HasDatabaseName("ix_accounting_periods_organization_status");

        builder.HasOne<FiscalYear>()
            .WithMany()
            .HasForeignKey(period => new
            {
                period.FiscalYearId,
                period.OrganizationId
            })
            .HasPrincipalKey(year => new
            {
                year.Id,
                year.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
