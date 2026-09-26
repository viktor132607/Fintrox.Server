using Fintrox.Domain.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class IntegrationFailureConfiguration
    : IEntityTypeConfiguration<IntegrationFailure>
{
    public void Configure(EntityTypeBuilder<IntegrationFailure> builder)
    {
        builder.ToTable(
            "integration_failures",
            DatabaseSchemas.Integration);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(item => item.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(item => item.ReferenceId)
            .HasColumnName("reference_id")
            .IsRequired();

        builder.Property(item => item.Reason)
            .HasColumnName("reason")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(item => item.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(item => item.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.Property(item => item.LastAttemptAtUtc)
            .HasColumnName("last_attempt_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(item => item.IsResolved)
            .HasColumnName("is_resolved")
            .IsRequired();

        builder.Property(item => item.ResolvedAtUtc)
            .HasColumnName("resolved_at_utc")
            .HasColumnType("timestamp with time zone");

        ConfigureAudit(builder);

        builder.HasIndex(item => new
            {
                item.OrganizationId,
                item.IsResolved,
                item.LastAttemptAtUtc
            })
            .HasDatabaseName(
                "ix_integration_failures_organization_resolved_attempt");

        builder.HasIndex(item => new
            {
                item.OrganizationId,
                item.Kind,
                item.ReferenceId
            })
            .HasFilter("is_resolved = FALSE")
            .IsUnique()
            .HasDatabaseName(
                "ux_integration_failures_open_reference");
    }

    private static void ConfigureAudit(
        EntityTypeBuilder<IntegrationFailure> builder)
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
