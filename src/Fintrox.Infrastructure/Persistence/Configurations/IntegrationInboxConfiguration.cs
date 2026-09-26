using Fintrox.Domain.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class IntegrationInboxConfiguration
    : IEntityTypeConfiguration<IntegrationInbox>
{
    public void Configure(EntityTypeBuilder<IntegrationInbox> builder)
    {
        builder.ToTable(
            "integration_inbox",
            DatabaseSchemas.Integration);

        builder.HasKey(item => item.Id);

        builder.HasAlternateKey(item => new
            {
                item.Id,
                item.OrganizationId
            })
            .HasName("ak_integration_inbox_id_organization");

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(item => item.SourceSystem)
            .HasColumnName("source_system")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.Operation)
            .HasColumnName("operation")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(item => item.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(item => item.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(item => item.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.Property(item => item.LastAttemptAtUtc)
            .HasColumnName("last_attempt_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(item => item.CompletedAtUtc)
            .HasColumnName("completed_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(item => item.ResultEntityType)
            .HasColumnName("result_entity_type")
            .HasMaxLength(100);

        builder.Property(item => item.ResultEntityId)
            .HasColumnName("result_entity_id");

        builder.Property(item => item.JournalEntryId)
            .HasColumnName("journal_entry_id");

        builder.Property(item => item.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(4000);

        ConfigureAudit(builder);

        builder.HasIndex(item => new
            {
                item.OrganizationId,
                item.SourceSystem,
                item.ExternalId,
                item.EventType
            })
            .IsUnique()
            .HasDatabaseName(
                "ux_integration_inbox_external_event");

        builder.HasIndex(item => new
            {
                item.OrganizationId,
                item.Status,
                item.CreatedAtUtc
            })
            .HasDatabaseName(
                "ix_integration_inbox_organization_status_created");
    }

    private static void ConfigureAudit(
        EntityTypeBuilder<IntegrationInbox> builder)
    {
        builder.Property(item => item.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(item => item.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(item => item.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(item => item.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");
    }
}
