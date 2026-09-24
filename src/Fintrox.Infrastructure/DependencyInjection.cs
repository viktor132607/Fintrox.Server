using Fintrox.Application.Organizations;
using Fintrox.Infrastructure.Organizations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fintrox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContextPool<FintroxDbContext>(
            options => PersistenceOptions.Configure(options, connectionString));

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();

        return services;
    }
}
