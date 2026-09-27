using Fintrox.Application.Accounting;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Application.Identity;
using Fintrox.Application.Organizations;
using Fintrox.Infrastructure.Accounting;
using Fintrox.Infrastructure.Audit;
using Fintrox.Infrastructure.Identity;
using Fintrox.Infrastructure.Organizations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fintrox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
        }

        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<FintroxDbContext>(
            (serviceProvider, options) =>
            {
                PersistenceOptions.Configure(options, connectionString);
                options.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
            });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);

                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<FintroxDbContext>();

        services.AddScoped<IAccountingTransactionRunner, EfAccountingTransactionRunner>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IFiscalCalendarRepository, FiscalCalendarRepository>();
        services.AddScoped<IJournalRepository, JournalRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<
            IOrganizationMembershipRepository,
            OrganizationMembershipRepository>();

        return services;
    }
}
