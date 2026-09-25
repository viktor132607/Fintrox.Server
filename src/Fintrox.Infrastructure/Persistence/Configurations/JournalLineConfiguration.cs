using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class JournalLineConfiguration
    : IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<JournalLine> builder)
    {
        builder.ToTable(
            "journal_lines",
            DatabaseSchemas.Accounting,
            table => table.HasCheckConstraint(
                "ck_journal_lines_single_sided_amount",
                "debit >= 0 AND credit >= 0 AND ((debit > 0 AND credit = 0) OR (credit > 0 AND debit = 0))"));

        builder.HasKey(line => line.Id);

        builder.Property(line => line.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(line => line.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(line => line.JournalEntryId)
            .HasColumnName("journal_entry_id")
            .IsRequired();

        builder.Property(line => line.LineNumber)
            .HasColumnName("line_number")
            .IsRequired();

        builder.Property(line => line.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(line => line.Debit)
            .HasColumnName("debit")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(line => line.Credit)
            .HasColumnName("credit")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(line => line.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(line => line.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(line => line.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(line => line.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(line => line.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(line => new
            {
                line.OrganizationId,
                line.JournalEntryId,
                line.LineNumber
            })
            .IsUnique()
            .HasDatabaseName("ux_journal_lines_entry_line_number");

        builder.HasIndex(line => new
            {
                line.OrganizationId,
                line.AccountId
            })
            .HasDatabaseName("ix_journal_lines_organization_account");

        builder.HasOne<JournalEntry>()
            .WithMany()
            .HasForeignKey(line => new
            {
                line.JournalEntryId,
                line.OrganizationId
            })
            .HasPrincipalKey(entry => new
            {
                entry.Id,
                entry.OrganizationId
            })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(line => new
            {
                line.AccountId,
                line.OrganizationId
            })
            .HasPrincipalKey(account => new
            {
                account.Id,
                account.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
