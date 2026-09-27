using Fintrox.Domain.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class WebhookDeliveryConfiguration
    : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(
        EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable(
            "webhook_deliveries",
            DatabaseSchemas.Integration);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(item => item.IntegrationEventId)
            .HasColumnName("integration_event_id")
            .IsRequired();

        builder.Property(item => item.WebhookSubscriptionId)
            .HasColumnName("webhook_subscription_id")
            .IsRequired();

        builder.Property(item => item.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(item => item.AttemptCount)
            .HasColumnName("attempt_count")
            .IsRequired();

        builder.Property(item => item.NextAttemptAtUtc)
            .HasColumnName("next_attempt_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(item => item.LastAttemptAtUtc)
            .HasColumnName("last_attempt_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(item => item.LastStatusCode)
            .HasColumnName("last_status_code");

        builder.Property(item => item.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(4000);

        builder.Property(item => item.DeliveredAtUtc)
            .HasColumnName("delivered_at_utc")
            .HasColumnType("timestamp with time zone");

        ConfigureAudit(builder);

        builder.HasIndex(item => new
            {
                item.Status,
                item.NextAttemptAtUtc
            })
            .HasDatabaseName(
                "ix_webhook_deliveries_status_next_attempt");

        builder.HasIndex(item => new
            {
                item.OrganizationId,
                item.WebhookSubscriptionId,
                item.CreatedAtUtc
            })
            .HasDatabaseName(
                "ix_webhook_deliveries_subscription_created");

        builder.HasIndex(item => new
            {
                item.IntegrationEventId,
                item.WebhookSubscriptionId
            })
            .IsUnique()
            .HasDatabaseName(
                "ux_webhook_deliveries_event_subscription");

        builder.HasOne<IntegrationEvent>()
            .WithMany()
            .HasForeignKey(item => new
            {
                item.IntegrationEventId,
                item.OrganizationId
            })
            .HasPrincipalKey(integrationEvent => new
            {
                integrationEvent.Id,
                integrationEvent.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WebhookSubscription>()
            .WithMany()
            .HasForeignKey(item => new
            {
                item.WebhookSubscriptionId,
                item.OrganizationId
            })
            .HasPrincipalKey(subscription => new
            {
                subscription.Id,
                subscription.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAudit(
        EntityTypeBuilder<WebhookDelivery> builder)
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
