using Fintrox.Domain.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class IntegrationEventConfiguration
    : IEntityTypeConfiguration<IntegrationEvent>
{
    public void Configure(EntityTypeBuilder<IntegrationEvent> builder)
    {
        builder.ToTable(
            "integration_events",
            DatabaseSchemas.Integration);

        builder.HasKey(item => item.Id);

        builder.HasAlternateKey(item => new
            {
                item.Id,
                item.OrganizationId
            })
            .HasName("ak_integration_events_id_organization");

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(item => item.InboxId)
            .HasColumnName("inbox_id");

        builder.Property(item => item.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.AggregateType)
            .HasColumnName("aggregate_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(item => item.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(item => item.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(item => item.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(item => item.DispatchedAtUtc)
            .HasColumnName("dispatched_at_utc")
            .HasColumnType("timestamp with time zone");

        ConfigureAudit(builder);

        builder.HasIndex(item => new
            {
                item.OrganizationId,
                item.EventType,
                item.OccurredAtUtc
            })
            .HasDatabaseName(
                "ix_integration_events_organization_type_occurred");

        builder.HasOne<IntegrationInbox>()
            .WithMany()
            .HasForeignKey(item => new
            {
                item.InboxId,
                item.OrganizationId
            })
            .HasPrincipalKey(inbox => new
            {
                inbox.Id,
                inbox.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAudit(
        EntityTypeBuilder<IntegrationEvent> builder)
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
