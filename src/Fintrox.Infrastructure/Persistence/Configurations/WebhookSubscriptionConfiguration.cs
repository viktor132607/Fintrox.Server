using Fintrox.Domain.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class WebhookSubscriptionConfiguration
    : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(
        EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable(
            "webhook_subscriptions",
            DatabaseSchemas.Integration);

        builder.HasKey(item => item.Id);

        builder.HasAlternateKey(item => new
            {
                item.Id,
                item.OrganizationId
            })
            .HasName(
                "ak_webhook_subscriptions_id_organization");

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(item => item.Name)
            .HasColumnName("name")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(item => item.TargetUrl)
            .HasColumnName("target_url")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(item => item.EventTypes)
            .HasColumnName("event_types")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(item => item.SigningSecretCiphertext)
            .HasColumnName("signing_secret_ciphertext")
            .HasMaxLength(4096)
            .IsRequired();

        builder.Property(item => item.SecretPrefix)
            .HasColumnName("secret_prefix")
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(item => item.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(item => item.SecretRotatedAtUtc)
            .HasColumnName("secret_rotated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        ConfigureAudit(builder);

        builder.HasIndex(item => new
            {
                item.OrganizationId,
                item.IsActive,
                item.Name
            })
            .HasDatabaseName(
                "ix_webhook_subscriptions_organization_active_name");
    }

    private static void ConfigureAudit(
        EntityTypeBuilder<WebhookSubscription> builder)
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
