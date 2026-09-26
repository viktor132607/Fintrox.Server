using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class JournalEntryConfiguration
    : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable(
            "journal_entries",
            DatabaseSchemas.Accounting,
            table =>
            {
                table.HasCheckConstraint(
                    "ck_journal_entries_posting_state",
                    "(status = 'Draft' AND number IS NULL AND posted_at_utc IS NULL) OR " +
                    "(status IN ('Posted', 'Reversed') AND number IS NOT NULL AND posted_at_utc IS NOT NULL)");

                table.HasCheckConstraint(
                    "ck_journal_entries_reversed_link",
                    "status <> 'Reversed' OR reversed_by_journal_entry_id IS NOT NULL");
            });

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

        builder.Property(entry => entry.ReversalOfJournalEntryId)
            .HasColumnName("reversal_of_journal_entry_id");

        builder.Property(entry => entry.ReversedByJournalEntryId)
            .HasColumnName("reversed_by_journal_entry_id");

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
                entry.Number
            })
            .IsUnique()
            .HasFilter("number IS NOT NULL")
            .HasDatabaseName("ux_journal_entries_organization_number");

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

        builder.HasIndex(entry => new
            {
                entry.OrganizationId,
                entry.Source,
                entry.ExternalReference
            })
            .IsUnique()
            .HasFilter("source = 'System' AND external_reference IS NOT NULL")
            .HasDatabaseName("ux_journal_entries_system_external_reference");

        builder.HasIndex(entry => new
            {
                entry.OrganizationId,
                entry.ReversalOfJournalEntryId
            })
            .IsUnique()
            .HasFilter("reversal_of_journal_entry_id IS NOT NULL")
            .HasDatabaseName("ux_journal_entries_reversal_of");

        builder.HasIndex(entry => new
            {
                entry.OrganizationId,
                entry.ReversedByJournalEntryId
            })
            .IsUnique()
            .HasFilter("reversed_by_journal_entry_id IS NOT NULL")
            .HasDatabaseName("ux_journal_entries_reversed_by");

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

        builder.HasOne<JournalEntry>()
            .WithMany()
            .HasForeignKey(entry => new
            {
                entry.ReversalOfJournalEntryId,
                entry.OrganizationId
            })
            .HasPrincipalKey(entry => new
            {
                entry.Id,
                entry.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<JournalEntry>()
            .WithMany()
            .HasForeignKey(entry => new
            {
                entry.ReversedByJournalEntryId,
                entry.OrganizationId
            })
            .HasPrincipalKey(entry => new
            {
                entry.Id,
                entry.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(entry => entry.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
