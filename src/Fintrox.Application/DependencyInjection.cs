using Fintrox.Application.Authorization;
using Fintrox.Application.Organizations;
using Microsoft.Extensions.DependencyInjection;

namespace Fintrox.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IOrganizationAccessService, OrganizationAccessService>();

        return services;
    }
}
