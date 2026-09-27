using Fintrox.Domain.Accounting;
using Fintrox.Domain.Partners;
using Fintrox.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable(
            "payments",
            DatabaseSchemas.Payments,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_payments_amount",
                    "amount > 0");

                table.HasCheckConstraint(
                    "ck_payments_allocated_amount",
                    "allocated_amount >= 0 AND allocated_amount <= amount");

                table.HasCheckConstraint(
                    "ck_payments_exchange_rate",
                    "exchange_rate IS NULL OR exchange_rate > 0");

                table.HasCheckConstraint(
                    "ck_payments_lifecycle",
                    "(status = 'Draft' AND internal_number IS NULL AND base_currency_id IS NULL AND base_currency_code IS NULL AND exchange_rate IS NULL AND confirmed_at_utc IS NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL) OR " +
                    "(status = 'Confirmed' AND internal_number IS NOT NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL AND confirmed_at_utc IS NOT NULL AND cancelled_at_utc IS NULL AND cancellation_reason IS NULL) OR " +
                    "(status = 'Cancelled' AND internal_number IS NOT NULL AND base_currency_id IS NOT NULL AND base_currency_code IS NOT NULL AND exchange_rate IS NOT NULL AND confirmed_at_utc IS NOT NULL AND cancelled_at_utc IS NOT NULL AND cancellation_reason IS NOT NULL)");
            });

        builder.HasKey(payment => payment.Id);

        builder.HasAlternateKey(payment => new
            {
                payment.Id,
                payment.OrganizationId
            })
            .HasName("ak_payments_id_organization");

        builder.Property(payment => payment.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(payment => payment.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(payment => payment.InternalNumber)
            .HasColumnName("internal_number")
            .HasMaxLength(40);

        builder.Property(payment => payment.Direction)
            .HasColumnName("direction")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(payment => payment.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(payment => payment.CounterpartyId)
            .HasColumnName("counterparty_id")
            .IsRequired();

        builder.Property(payment => payment.CounterpartyName)
            .HasColumnName("counterparty_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(payment => payment.CounterpartyRegistrationNumber)
            .HasColumnName("counterparty_registration_number")
            .HasMaxLength(64);

        builder.Property(payment => payment.CounterpartyVatNumber)
            .HasColumnName("counterparty_vat_number")
            .HasMaxLength(64);

        builder.Property(payment => payment.PaymentDate)
            .HasColumnName("payment_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(payment => payment.Method)
            .HasColumnName("method")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(payment => payment.CurrencyId)
            .HasColumnName("currency_id")
            .IsRequired();

        builder.Property(payment => payment.BaseCurrencyId)
            .HasColumnName("base_currency_id");

        builder.Property(payment => payment.CurrencyCode)
            .HasColumnName("currency_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(payment => payment.BaseCurrencyCode)
            .HasColumnName("base_currency_code")
            .HasMaxLength(3);

        builder.Property(payment => payment.ExchangeRate)
            .HasColumnName("exchange_rate")
            .HasPrecision(22, 10);

        builder.Property(payment => payment.Amount)
            .HasColumnName("amount")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(payment => payment.AllocatedAmount)
            .HasColumnName("allocated_amount")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Ignore(payment => payment.UnallocatedAmount);

        builder.Property(payment => payment.Reference)
            .HasColumnName("reference")
            .HasMaxLength(120);

        builder.Property(payment => payment.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(payment => payment.ConfirmedAtUtc)
            .HasColumnName("confirmed_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(payment => payment.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(payment => payment.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(500);

        builder.Property(payment => payment.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(payment => payment.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(payment => payment.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(payment => payment.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(payment => new
            {
                payment.OrganizationId,
                payment.InternalNumber
            })
            .IsUnique()
            .HasFilter("internal_number IS NOT NULL")
            .HasDatabaseName("ux_payments_organization_internal_number");

        builder.HasIndex(payment => new
            {
                payment.OrganizationId,
                payment.PaymentDate
            })
            .HasDatabaseName("ix_payments_organization_date");

        builder.HasIndex(payment => new
            {
                payment.OrganizationId,
                payment.Status,
                payment.PaymentDate
            })
            .HasDatabaseName("ix_payments_organization_status_date");

        builder.HasIndex(payment => new
            {
                payment.OrganizationId,
                payment.CounterpartyId,
                payment.PaymentDate
            })
            .HasDatabaseName("ix_payments_organization_counterparty_date");

        builder.HasOne<Counterparty>()
            .WithMany()
            .HasForeignKey(payment => new
            {
                payment.CounterpartyId,
                payment.OrganizationId
            })
            .HasPrincipalKey(counterparty => new
            {
                counterparty.Id,
                counterparty.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(payment => new
            {
                payment.CurrencyId,
                payment.OrganizationId
            })
            .HasPrincipalKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(payment => new
            {
                payment.BaseCurrencyId,
                payment.OrganizationId
            })
            .HasPrincipalKey(currency => new
            {
                currency.Id,
                currency.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(payment => payment.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
