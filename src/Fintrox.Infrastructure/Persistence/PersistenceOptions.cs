using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Persistence;

internal static class PersistenceOptions
{
    public static void Configure(
        DbContextOptionsBuilder options,
        string connectionString)
    {
        options.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(FintroxDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(2),
                    errorCodesToAdd: null);
            });
    }
}
