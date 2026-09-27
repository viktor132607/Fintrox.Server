using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fintrox.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", DatabaseSchemas.Accounting);

        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(account => account.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(account => account.Code)
            .HasColumnName("code")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(account => account.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(account => account.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(account => account.ParentAccountId)
            .HasColumnName("parent_account_id");

        builder.Property(account => account.IsAnalytical)
            .HasColumnName("is_analytical")
            .IsRequired();

        builder.Property(account => account.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(account => account.AllowManualPosting)
            .HasColumnName("allow_manual_posting")
            .IsRequired();

        builder.Property(account => account.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(account => account.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(account => account.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(account => account.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasIndex(account => new
            {
                account.OrganizationId,
                account.Code
            })
            .IsUnique()
            .HasDatabaseName("ux_accounts_organization_code");

        builder.HasIndex(account => new
            {
                account.OrganizationId,
                account.ParentAccountId
            })
            .HasDatabaseName("ix_accounts_organization_parent");

        builder.HasIndex(account => new
            {
                account.OrganizationId,
                account.Type,
                account.IsActive
            })
            .HasDatabaseName("ix_accounts_organization_type_active");

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(account => account.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Fintrox.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(account => account.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
