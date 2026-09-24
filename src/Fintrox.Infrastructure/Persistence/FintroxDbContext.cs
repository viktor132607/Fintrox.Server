using Fintrox.Domain.Common;
using Fintrox.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Persistence;

public sealed class FintroxDbContext(DbContextOptions<FintroxDbContext> options)
    : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();

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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FintroxDbContext).Assembly);
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
