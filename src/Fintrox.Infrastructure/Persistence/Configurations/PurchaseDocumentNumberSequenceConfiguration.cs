using Fintrox.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class PurchaseDocumentNumberSequenceConfiguration
    : IEntityTypeConfiguration<PurchaseDocumentNumberSequence>
{
    public void Configure(
        EntityTypeBuilder<PurchaseDocumentNumberSequence> builder)
    {
        builder.ToTable(
            "purchase_document_number_sequences",
            DatabaseSchemas.Purchases,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_purchase_document_sequences_year",
                    "calendar_year >= 1");

                table.HasCheckConstraint(
                    "ck_purchase_document_sequences_last_number",
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
