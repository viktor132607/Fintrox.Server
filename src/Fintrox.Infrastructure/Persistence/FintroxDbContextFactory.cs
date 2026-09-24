using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fintrox.Infrastructure.Persistence;

public sealed class FintroxDbContextFactory : IDesignTimeDbContextFactory<FintroxDbContext>
{
    private const string LocalConnectionString =
        "Host=localhost;Port=5432;Database=fintrox;Username=fintrox;Password=fintrox_dev";

    public FintroxDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? LocalConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<FintroxDbContext>();
        PersistenceOptions.Configure(optionsBuilder, connectionString);

        return new FintroxDbContext(optionsBuilder.Options);
    }
}
