using Fintrox.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log", DatabaseSchemas.Audit);

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(entry => entry.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(entry => entry.UserId)
            .HasColumnName("user_id");

        builder.Property(entry => entry.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(entry => entry.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(entry => entry.Action)
            .HasColumnName("action")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(entry => entry.ChangesJson)
            .HasColumnName("changes_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(entry => entry.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(128);

        builder.Property(entry => entry.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(entry => new { entry.OrganizationId, entry.OccurredAtUtc })
            .HasDatabaseName("ix_audit_log_organization_occurred");

        builder.HasIndex(entry => new { entry.EntityType, entry.EntityId })
            .HasDatabaseName("ix_audit_log_entity");

        builder.HasIndex(entry => new { entry.UserId, entry.OccurredAtUtc })
            .HasDatabaseName("ix_audit_log_user_occurred");
    }
}
