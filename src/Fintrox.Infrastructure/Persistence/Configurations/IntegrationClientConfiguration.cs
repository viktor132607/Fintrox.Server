using Fintrox.Domain.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class IntegrationClientConfiguration
    : IEntityTypeConfiguration<IntegrationClient>
{
    public void Configure(EntityTypeBuilder<IntegrationClient> builder)
    {
        builder.ToTable(
            "integration_clients",
            DatabaseSchemas.Integration);

        builder.HasKey(client => client.Id);

        builder.HasAlternateKey(client => new
            {
                client.Id,
                client.OrganizationId
            })
            .HasName("ak_integration_clients_id_organization");

        builder.Property(client => client.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(client => client.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(client => client.ClientId)
            .HasColumnName("client_id")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(client => client.Name)
            .HasColumnName("name")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(client => client.SecretHash)
            .HasColumnName("secret_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(client => client.SecretPrefix)
            .HasColumnName("secret_prefix")
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(client => client.Scopes)
            .HasColumnName("scopes")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(client => client.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(client => client.SecretRotatedAtUtc)
            .HasColumnName("secret_rotated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(client => client.LastUsedAtUtc)
            .HasColumnName("last_used_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(client => client.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(client => client.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(client => client.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(client => client.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(client => client.ClientId)
            .IsUnique()
            .HasDatabaseName("ux_integration_clients_client_id");

        builder.HasIndex(client => new
            {
                client.OrganizationId,
                client.IsActive,
                client.Name
            })
            .HasDatabaseName(
                "ix_integration_clients_organization_active_name");

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(client => client.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
