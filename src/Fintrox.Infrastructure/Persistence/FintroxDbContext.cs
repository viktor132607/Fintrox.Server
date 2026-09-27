using Fintrox.Domain.Accounting;
using Fintrox.Domain.Common;
using Fintrox.Domain.Organizations;
using Fintrox.Infrastructure.Audit;
using Fintrox.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Persistence;

public sealed class FintroxDbContext(DbContextOptions<FintroxDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();

    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();

    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    public DbSet<JournalLine> JournalLines => Set<JournalLine>();

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<OrganizationMembership> OrganizationMemberships =>
        Set<OrganizationMembership>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateOrganizationScopes();
        ValidateAuditLogImmutability();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ValidateOrganizationScopes();
        ValidateAuditLogImmutability();

        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentityTables(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(FintroxDbContext).Assembly);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>()
            .ToTable("users", DatabaseSchemas.Identity);

        builder.Entity<IdentityUserClaim<Guid>>()
            .ToTable("user_claims", DatabaseSchemas.Identity);

        builder.Entity<IdentityUserLogin<Guid>>()
            .ToTable("user_logins", DatabaseSchemas.Identity);

        builder.Entity<IdentityUserToken<Guid>>()
            .ToTable("user_tokens", DatabaseSchemas.Identity);
    }

    private void ValidateOrganizationScopes()
    {
        var invalidEntry = ChangeTracker
            .Entries<IOrganizationScopedEntity>()
            .FirstOrDefault(entry =>
                entry.State is EntityState.Added or EntityState.Modified &&
                entry.Entity.OrganizationId == Guid.Empty);

        if (invalidEntry is not null)
        {
            throw new InvalidOperationException(
                $"Organization-scoped entity '{invalidEntry.Metadata.ClrType.Name}' has an empty OrganizationId.");
        }
    }

    private void ValidateAuditLogImmutability()
    {
        var mutableAuditEntry = ChangeTracker
            .Entries<AuditLogEntry>()
            .FirstOrDefault(entry =>
                entry.State is EntityState.Modified or EntityState.Deleted);

        if (mutableAuditEntry is not null)
        {
            throw new InvalidOperationException(
                "Audit log entries are append-only and cannot be modified or deleted.");
        }
    }
}
