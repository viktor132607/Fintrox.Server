using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class JournalEntryConfiguration
    : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries", DatabaseSchemas.Accounting);

        builder.HasKey(entry => entry.Id);

        builder.HasAlternateKey(entry => new
            {
                entry.Id,
                entry.OrganizationId
            })
            .HasName("ak_journal_entries_id_organization");

        builder.Property(entry => entry.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(entry => entry.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(entry => entry.Number)
            .HasColumnName("number")
            .HasMaxLength(40);

        builder.Property(entry => entry.PostingDate)
            .HasColumnName("posting_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(entry => entry.DocumentDate)
            .HasColumnName("document_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(entry => entry.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(entry => entry.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(entry => entry.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(entry => entry.ExternalReference)
            .HasColumnName("external_reference")
            .HasMaxLength(160);

        builder.Property(entry => entry.FiscalPeriodId)
            .HasColumnName("fiscal_period_id")
            .IsRequired();

        builder.Property(entry => entry.PostedAtUtc)
            .HasColumnName("posted_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(entry => entry.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(entry => entry.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(entry => entry.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(entry => entry.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(entry => new
            {
                entry.OrganizationId,
                entry.PostingDate
            })
            .HasDatabaseName("ix_journal_entries_organization_posting_date");

        builder.HasIndex(entry => new
            {
                entry.OrganizationId,
                entry.Status,
                entry.PostingDate
            })
            .HasDatabaseName("ix_journal_entries_organization_status_date");

        builder.HasIndex(entry => new
            {
                entry.OrganizationId,
                entry.ExternalReference
            })
            .HasDatabaseName("ix_journal_entries_organization_external_reference");

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(entry => new
            {
                entry.FiscalPeriodId,
                entry.OrganizationId
            })
            .HasPrincipalKey(period => new
            {
                period.Id,
                period.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(entry => entry.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
