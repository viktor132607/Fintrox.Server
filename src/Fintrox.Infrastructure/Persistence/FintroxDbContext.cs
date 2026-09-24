using Fintrox.Domain.Common;
using Fintrox.Domain.Organizations;
using Fintrox.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Persistence;

public sealed class FintroxDbContext(DbContextOptions<FintroxDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<OrganizationMembership> OrganizationMemberships =>
        Set<OrganizationMembership>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateOrganizationScopes();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ValidateOrganizationScopes();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIdentityTables(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FintroxDbContext).Assembly);
    }

    private static void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>()
            .ToTable("users", DatabaseSchemas.Identity);

        modelBuilder.Entity<IdentityUserClaim<Guid>>()
            .ToTable("user_claims", DatabaseSchemas.Identity);

        modelBuilder.Entity<IdentityUserLogin<Guid>>()
            .ToTable("user_logins", DatabaseSchemas.Identity);

        modelBuilder.Entity<IdentityUserToken<Guid>>()
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
}
