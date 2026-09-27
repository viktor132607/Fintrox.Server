using Fintrox.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class PaymentNumberSequenceConfiguration
    : IEntityTypeConfiguration<PaymentNumberSequence>
{
    public void Configure(
        EntityTypeBuilder<PaymentNumberSequence> builder)
    {
        builder.ToTable(
            "payment_number_sequences",
            DatabaseSchemas.Payments,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_payment_sequences_year",
                    "calendar_year >= 1");

                table.HasCheckConstraint(
                    "ck_payment_sequences_last_number",
                    "last_number >= 0");
            });

        builder.HasKey(sequence => new
            {
                sequence.OrganizationId,
                sequence.CalendarYear,
                sequence.Direction
            });

        builder.Property(sequence => sequence.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(sequence => sequence.CalendarYear)
            .HasColumnName("calendar_year")
            .IsRequired();

        builder.Property(sequence => sequence.Direction)
            .HasColumnName("direction")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(sequence => sequence.LastNumber)
            .HasColumnName("last_number")
            .IsRequired();

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(sequence => sequence.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
