using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Application.Organizations;
using Microsoft.Extensions.DependencyInjection;

namespace Fintrox.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IFiscalCalendarService, FiscalCalendarService>();
        services.AddScoped<IJournalService, JournalService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IOrganizationMemberService, OrganizationMemberService>();
        services.AddScoped<IOrganizationAccessService, OrganizationAccessService>();

        return services;
    }
}
