using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Api;

[TestClass]
public sealed class ApiRequestSecurityHttpTests
{
    private const string AllowedOrigin = "https://app.fintrox.example";
    private const string SigningKey =
        "test-only-api-request-security-signing-key-2026-not-for-production";

    [TestMethod]
    public async Task ConfiguredOriginReceivesPreflightAndNormalResponseHeaders()
    {
        using var factory = Factory();
        using var client = Client(factory);

        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        preflight.Headers.TryAddWithoutValidation("Origin", AllowedOrigin);
        preflight.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "POST");
        preflight.Headers.TryAddWithoutValidation(
            "Access-Control-Request-Headers", "content-type,authorization");
        using var response = await client.SendAsync(preflight);
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        Assert.AreEqual(AllowedOrigin,
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Credentials"));

        using var get = new HttpRequestMessage(HttpMethod.Get, "/api/v1/system/info");
        get.Headers.TryAddWithoutValidation("Origin", AllowedOrigin);
        using var actual = await client.SendAsync(get);
        Assert.AreEqual(HttpStatusCode.OK, actual.StatusCode);
        Assert.AreEqual(AllowedOrigin,
            actual.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [TestMethod]
    public async Task UnknownOriginIsNeverEchoedInPreflightOrNormalResponses()
    {
        using var factory = Factory();
        using var client = Client(factory);

        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        preflight.Headers.TryAddWithoutValidation("Origin", "https://attacker.example");
        preflight.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "POST");
        using var response = await client.SendAsync(preflight);
        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));

        using var get = new HttpRequestMessage(HttpMethod.Get, "/api/v1/system/info");
        get.Headers.TryAddWithoutValidation("Origin", "https://attacker.example");
        using var actual = await client.SendAsync(get);
        Assert.AreEqual(HttpStatusCode.OK, actual.StatusCode);
        Assert.IsFalse(actual.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [TestMethod]
    public async Task NoConfiguredOriginsDeniesCrossOriginRequestsByDefault()
    {
        using var factory = Factory(includeOrigin: false);
        using var client = Client(factory);
        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        preflight.Headers.TryAddWithoutValidation("Origin", AllowedOrigin);
        preflight.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "POST");
        using var response = await client.SendAsync(preflight);
        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [TestMethod]
    public async Task LoginLimitReturns429WithoutThrottlingUnrelatedReadEndpoints()
    {
        using var factory = Factory();
        using var client = Client(factory);
        for (var i = 0; i < 10; i++)
        {
            using var attempt = await InvalidPost(client, "/api/v1/auth/login");
            Assert.AreEqual(HttpStatusCode.BadRequest, attempt.StatusCode);
        }

        using var blocked = await InvalidPost(client, "/api/v1/auth/login");
        Assert.AreEqual(HttpStatusCode.TooManyRequests, blocked.StatusCode);

        using var system = await client.GetAsync("/api/v1/system/info");
        Assert.AreEqual(HttpStatusCode.OK, system.StatusCode);

        using var refresh = await InvalidPost(client, "/api/v1/auth/refresh");
        Assert.AreEqual(HttpStatusCode.BadRequest, refresh.StatusCode);
    }

    [TestMethod]
    public async Task RegistrationAndIntegrationTokensHaveIndependentLimits()
    {
        using var factory = Factory();
        using var client = Client(factory);
        for (var i = 0; i < 5; i++)
        {
            using var attempt = await InvalidPost(client, "/api/v1/auth/register");
            Assert.AreEqual(HttpStatusCode.BadRequest, attempt.StatusCode);
        }
        using var registrationBlocked = await InvalidPost(client, "/api/v1/auth/register");
        Assert.AreEqual(HttpStatusCode.TooManyRequests, registrationBlocked.StatusCode);

        for (var i = 0; i < 20; i++)
        {
            using var attempt = await InvalidPost(client, "/api/v1/integrations/token");
            Assert.AreEqual(HttpStatusCode.BadRequest, attempt.StatusCode);
        }
        using var integrationBlocked = await InvalidPost(client, "/api/v1/integrations/token");
        Assert.AreEqual(HttpStatusCode.TooManyRequests, integrationBlocked.StatusCode);
    }

    [TestMethod]
    public async Task SpoofedForwardedAddressCannotResetLoginLimiter()
    {
        using var factory = Factory();
        using var client = Client(factory);
        for (var i = 0; i < 10; i++)
        {
            using var attempt = await InvalidPost(client, "/api/v1/auth/login");
            Assert.AreEqual(HttpStatusCode.BadRequest, attempt.StatusCode);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = new StringContent("{", Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("X-Forwarded-For", "203.0.113.123");
        using var response = await client.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> InvalidPost(HttpClient client, string path)
    {
        using var content = new StringContent("{", Encoding.UTF8, "application/json");
        return await client.PostAsync(path, content);
    }

    private static HttpClient Client(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private static WebApplicationFactory<Program> Factory(bool includeOrigin = true) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // CORS is configured while Program registers services, before
            // late WebApplicationFactory app-configuration callbacks apply.
            // Supply the allowed test origin through early host settings.
            if (includeOrigin)
                builder.UseSetting("Cors:AllowedOrigins:0", AllowedOrigin);
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                var values = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        "Host=localhost;Database=fintrox_security_test;Username=test;Password=unused",
                    ["Jwt:SigningKey"] = SigningKey,
                    ["Jwt:Issuer"] = "Fintrox",
                    ["Jwt:Audience"] = "Fintrox.Client"
                };
                if (includeOrigin) values["Cors:AllowedOrigins:0"] = AllowedOrigin;
                configuration.AddInMemoryCollection(values);
            });
            builder.ConfigureTestServices(services =>
            {
                // Rate-limit and CORS HTTP tests must not contact PostgreSQL.
                var worker = services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(IHostedService) &&
                    descriptor.ImplementationType?.Name == "WebhookDeliveryWorker");
                if (worker is not null) services.Remove(worker);
            });
        });
}
