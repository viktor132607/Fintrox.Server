using Fintrox.Domain.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class IntegrationRequestRecordConfiguration
    : IEntityTypeConfiguration<IntegrationRequestRecord>
{
    public void Configure(
        EntityTypeBuilder<IntegrationRequestRecord> builder)
    {
        builder.ToTable(
            "integration_requests",
            DatabaseSchemas.Integration);

        builder.HasKey(request => request.Id);

        builder.Property(request => request.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(request => request.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(request => request.IntegrationClientId)
            .HasColumnName("integration_client_id")
            .IsRequired();

        builder.Property(request => request.SourceSystem)
            .HasColumnName("source_system")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(request => request.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(request => request.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(request => request.RequestMethod)
            .HasColumnName("request_method")
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(request => request.RequestPath)
            .HasColumnName("request_path")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(request => request.RequestHash)
            .HasColumnName("request_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(request => request.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(request => request.ResponseStatusCode)
            .HasColumnName("response_status_code");

        builder.Property(request => request.ResponseContentType)
            .HasColumnName("response_content_type")
            .HasMaxLength(256);

        builder.Property(request => request.ResponseBody)
            .HasColumnName("response_body")
            .HasColumnType("text");

        builder.Property(request => request.ResourceReference)
            .HasColumnName("resource_reference")
            .HasMaxLength(2048);

        builder.Property(request => request.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(request => request.CompletedAtUtc)
            .HasColumnName("completed_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(request => new
            {
                request.OrganizationId,
                request.SourceSystem,
                request.ExternalId,
                request.EventType
            })
            .IsUnique()
            .HasDatabaseName(
                "ux_integration_requests_external_event");

        builder.HasIndex(request => new
            {
                request.OrganizationId,
                request.CreatedAtUtc
            })
            .HasDatabaseName(
                "ix_integration_requests_organization_created");

        builder.HasOne<IntegrationClient>()
            .WithMany()
            .HasForeignKey(request => new
            {
                request.IntegrationClientId,
                request.OrganizationId
            })
            .HasPrincipalKey(client => new
            {
                client.Id,
                client.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
