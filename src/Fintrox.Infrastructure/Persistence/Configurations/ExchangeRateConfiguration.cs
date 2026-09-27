using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class ExchangeRateConfiguration
    : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable(
            "exchange_rates",
            DatabaseSchemas.Accounting,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_exchange_rates_positive_rate",
                    "rate > 0");

                table.HasCheckConstraint(
                    "ck_exchange_rates_distinct_currencies",
                    "base_currency_id <> quote_currency_id");
            });

        builder.HasKey(rate => rate.Id);

        builder.Property(rate => rate.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(rate => rate.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(rate => rate.BaseCurrencyId)
            .HasColumnName("base_currency_id")
            .IsRequired();

        builder.Property(rate => rate.QuoteCurrencyId)
            .HasColumnName("quote_currency_id")
            .IsRequired();

        builder.Property(rate => rate.EffectiveDate)
            .HasColumnName("effective_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(rate => rate.Rate)
            .HasColumnName("rate")
            .HasPrecision(22, 10)
            .IsRequired();

        builder.Property(rate => rate.Source)
            .HasColumnName("source")
            .HasMaxLength(120);

        builder.Property(rate => rate.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(rate => rate.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(rate => rate.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(rate => rate.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(rate => new
            {
                rate.OrganizationId,
                rate.BaseCurrencyId,
                rate.QuoteCurrencyId,
                rate.EffectiveDate
            })
            .IsUnique()
            .HasDatabaseName("ux_exchange_rates_pair_date");

        builder.HasIndex(rate => new
            {
                rate.OrganizationId,
                rate.EffectiveDate
            })
            .HasDatabaseName("ix_exchange_rates_organization_date");

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(rate => new
            {
                rate.BaseCurrencyId,
                rate.OrganizationId
            })
            .HasPrincipalKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(rate => new
            {
                rate.QuoteCurrencyId,
                rate.OrganizationId
            })
            .HasPrincipalKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(rate => rate.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
