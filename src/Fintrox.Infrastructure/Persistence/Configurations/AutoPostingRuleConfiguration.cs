using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class AutoPostingRuleConfiguration
    : IEntityTypeConfiguration<AutoPostingRule>
{
    public void Configure(EntityTypeBuilder<AutoPostingRule> builder)
    {
        builder.ToTable("auto_posting_rules", DatabaseSchemas.Accounting);

        builder.HasKey(rule => rule.Id);

        builder.HasAlternateKey(rule => new
            {
                rule.Id,
                rule.OrganizationId
            })
            .HasName("ak_auto_posting_rules_id_organization");

        builder.Property(rule => rule.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(rule => rule.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(rule => rule.Component)
            .HasColumnName("component")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(rule => rule.MatchKind)
            .HasColumnName("match_kind")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(rule => rule.MatchValue)
            .HasColumnName("match_value")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(rule => rule.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(rule => rule.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(rule => rule.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(rule => rule.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(rule => rule.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(rule => rule.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(rule => new
            {
                rule.OrganizationId,
                rule.Component,
                rule.MatchKind,
                rule.MatchValue
            })
            .IsUnique()
            .HasDatabaseName("ux_auto_posting_rules_resolution");

        builder.HasIndex(rule => new
            {
                rule.OrganizationId,
                rule.IsActive,
                rule.Component
            })
            .HasDatabaseName("ix_auto_posting_rules_active_component");

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(rule => new
            {
                rule.AccountId,
                rule.OrganizationId
            })
            .HasPrincipalKey(account => new
            {
                account.Id,
                account.OrganizationId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(rule => rule.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
