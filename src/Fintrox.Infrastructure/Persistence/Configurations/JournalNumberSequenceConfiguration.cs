using Fintrox.Domain.Accounting;
using Fintrox.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class JournalNumberSequenceConfiguration
    : IEntityTypeConfiguration<JournalNumberSequence>
{
    public void Configure(EntityTypeBuilder<JournalNumberSequence> builder)
    {
        builder.ToTable(
            "journal_number_sequences",
            DatabaseSchemas.Accounting);

        builder.HasKey(sequence => new
            {
                sequence.OrganizationId,
                sequence.FiscalYearId
            });

        builder.Property(sequence => sequence.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(sequence => sequence.FiscalYearId)
            .HasColumnName("fiscal_year_id")
            .IsRequired();

        builder.Property(sequence => sequence.LastNumber)
            .HasColumnName("last_number")
            .IsRequired();

        builder.HasOne<FiscalYear>()
            .WithMany()
            .HasForeignKey(sequence => new
            {
                sequence.FiscalYearId,
                sequence.OrganizationId
            })
            .HasPrincipalKey(year => new
            {
                year.Id,
                year.OrganizationId
            })
            .OnDelete(DeleteBehavior.Cascade);
    }
}
