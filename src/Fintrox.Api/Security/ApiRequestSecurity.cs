using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Fintrox.Api.Security;

/// <summary>
/// Browser origins must be explicitly configured; authentication limits are
/// partitioned by the server-observed remote address, never client-supplied headers.
/// </summary>
public static class ApiRequestSecurity
{
    public const string CorsPolicy = "FintroxFrontend";
    public const string RegisterLimit = "AuthRegister";
    public const string LoginLimit = "AuthLogin";
    public const string RefreshLimit = "AuthRefresh";
    public const string RevokeLimit = "AuthRevoke";
    public const string IntegrationTokenLimit = "IntegrationToken";

    public static IServiceCollection AddApiRequestSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        foreach (var origin in allowedOrigins)
        {
            if (string.IsNullOrWhiteSpace(origin) ||
                !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                !string.Equals(
                    uri.GetLeftPart(UriPartial.Authority), origin, StringComparison.Ordinal) ||
                uri.Host.Contains('*'))
            {
                throw new InvalidOperationException(
                    "Cors:AllowedOrigins must contain exact HTTP(S) origins without paths, wildcards or trailing slashes.");
            }
        }

        services.AddCors(options =>
            options.AddPolicy(CorsPolicy, policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            AddIpLimit(options, RegisterLimit, 5, TimeSpan.FromHours(1));
            AddIpLimit(options, LoginLimit, 10, TimeSpan.FromMinutes(1));
            AddIpLimit(options, RefreshLimit, 30, TimeSpan.FromMinutes(1));
            AddIpLimit(options, RevokeLimit, 30, TimeSpan.FromMinutes(1));
            AddIpLimit(options, IntegrationTokenLimit, 20, TimeSpan.FromMinutes(1));
        });

        return services;
    }

    private static void AddIpLimit(
        RateLimiterOptions options,
        string policyName,
        int permitLimit,
        TimeSpan window)
    {
        options.AddPolicy(policyName, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?
                    .MapToIPv6().ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }
}
