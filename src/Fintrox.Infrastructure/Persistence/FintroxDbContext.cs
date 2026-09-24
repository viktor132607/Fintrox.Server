using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Persistence;

public sealed class FintroxDbContext(DbContextOptions<FintroxDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FintroxDbContext).Assembly);
    }
}
