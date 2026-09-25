using Fintrox.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class SalesInvoiceNumberSequenceConfiguration
    : IEntityTypeConfiguration<SalesInvoiceNumberSequence>
{
    public void Configure(
        EntityTypeBuilder<SalesInvoiceNumberSequence> builder)
    {
        builder.ToTable(
            "sales_invoice_number_sequences",
            DatabaseSchemas.Sales,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_sales_invoice_sequences_year",
                    "calendar_year >= 1");

                table.HasCheckConstraint(
                    "ck_sales_invoice_sequences_last_number",
                    "last_number >= 0");
            });

        builder.HasKey(sequence => new
            {
                sequence.OrganizationId,
                sequence.CalendarYear
            });

        builder.Property(sequence => sequence.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(sequence => sequence.CalendarYear)
            .HasColumnName("calendar_year")
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
