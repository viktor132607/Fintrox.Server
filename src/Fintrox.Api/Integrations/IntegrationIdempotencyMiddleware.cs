using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Fintrox.Application.Authorization;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Application.Integrations;

namespace Fintrox.Api.Integrations;

public sealed class IntegrationIdempotencyMiddleware(
    RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ICurrentOrganization currentOrganization,
        IIntegrationIdempotencyExecutor executor)
    {
        if (!IsIntegrationMutation(context))
        {
            await next(context);
            return;
        }

        var metadata = await TryCreateMetadataAsync(
            context,
            currentOrganization.OrganizationId);

        if (metadata is null)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Missing integration idempotency metadata",
                $"Mutating integration requests require '{IntegrationIdempotencyHeaders.SourceSystem}', '{IntegrationIdempotencyHeaders.ExternalId}' and '{IntegrationIdempotencyHeaders.EventType}'.");
            return;
        }

        var originalBody = context.Response.Body;
        await using var capture = new MemoryStream();
        context.Response.Body = capture;

        try
        {
            IntegrationIdempotencyOutcome outcome;

            try
            {
                outcome = await executor.ExecuteAsync(
                    metadata,
                    async cancellationToken =>
                    {
                        capture.SetLength(0);
                        capture.Position = 0;

                        if (context.Request.Body.CanSeek)
                        {
                            context.Request.Body.Position = 0;
                        }

                        context.Response.Clear();
                        context.Response.Body = capture;

                        await next(context);

                        capture.Position = 0;
                        using var reader = new StreamReader(
                            capture,
                            Encoding.UTF8,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true);

                        var body = await reader.ReadToEndAsync(
                            cancellationToken);

                        var resourceReference =
                            context.Response.Headers["Location"].FirstOrDefault();

                        return new IntegrationIdempotencyExecutionResult(
                            context.Response.StatusCode,
                            context.Response.ContentType,
                            body,
                            resourceReference);
                    },
                    context.RequestAborted);
            }
            catch (IntegrationIdempotencyConflictException exception)
            {
                context.Response.Body = originalBody;
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    "Integration idempotency conflict",
                    exception.Message);
                return;
            }

            context.Response.Body = originalBody;
            context.Response.StatusCode = outcome.StatusCode;

            if (!string.IsNullOrWhiteSpace(outcome.ContentType))
            {
                context.Response.ContentType = outcome.ContentType;
            }

            if (!string.IsNullOrWhiteSpace(outcome.ResourceReference))
            {
                context.Response.Headers["Location"] =
                    outcome.ResourceReference;
            }

            if (outcome.Replayed)
            {
                context.Response.Headers[
                    IntegrationIdempotencyHeaders.Replayed] = "true";
            }

            if (!string.IsNullOrEmpty(outcome.Body))
            {
                await context.Response.WriteAsync(
                    outcome.Body,
                    context.RequestAborted);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static bool IsIntegrationMutation(
        HttpContext context)
    {
        var actorType = context.User.FindFirstValue(
            IntegrationClaims.ActorType);

        if (!string.Equals(
                actorType,
                IntegrationClaims.IntegrationActor,
                StringComparison.Ordinal))
        {
            return false;
        }

        return !HttpMethods.IsGet(context.Request.Method) &&
               !HttpMethods.IsHead(context.Request.Method) &&
               !HttpMethods.IsOptions(context.Request.Method) &&
               !HttpMethods.IsTrace(context.Request.Method);
    }

    private static async Task<IntegrationIdempotencyRequest?> TryCreateMetadataAsync(
        HttpContext context,
        Guid? organizationId)
    {
        if (organizationId is null)
        {
            return null;
        }

        var subject = context.User.FindFirstValue(
            JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(
                subject,
                out var integrationClientId))
        {
            return null;
        }

        var sourceSystem = ReadHeader(
            context,
            IntegrationIdempotencyHeaders.SourceSystem,
            100);
        var externalId = ReadHeader(
            context,
            IntegrationIdempotencyHeaders.ExternalId,
            200);
        var eventType = ReadHeader(
            context,
            IntegrationIdempotencyHeaders.EventType,
            100);

        if (sourceSystem is null ||
            externalId is null ||
            eventType is null)
        {
            return null;
        }

        var requestPath = string.Concat(
            context.Request.PathBase.Value,
            context.Request.Path.Value,
            context.Request.QueryString.Value);

        if (requestPath.Length > 2048)
        {
            return null;
        }

        return new IntegrationIdempotencyRequest(
            organizationId.Value,
            integrationClientId,
            sourceSystem,
            externalId,
            eventType,
            context.Request.Method,
            requestPath,
            await ComputeRequestHashAsync(
                context.Request,
                requestPath,
                context.RequestAborted));
    }

    private static string? ReadHeader(
        HttpContext context,
        string name,
        int maxLength)
    {
        var value = context.Request.Headers[name]
            .FirstOrDefault()?
            .Trim();

        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > maxLength)
        {
            return null;
        }

        return value;
    }

    private static async Task<string> ComputeRequestHashAsync(
        HttpRequest request,
        string requestPath,
        CancellationToken cancellationToken)
    {
        request.EnableBuffering();

        using var hash = IncrementalHash.CreateHash(
            HashAlgorithmName.SHA256);

        var prefix = Encoding.UTF8.GetBytes(
            $"{request.Method}\n{requestPath}\n");
        hash.AppendData(prefix);

        request.Body.Position = 0;

        var buffer = new byte[8192];
        int read;

        while ((read = await request.Body.ReadAsync(
                    buffer,
                    cancellationToken)) > 0)
        {
            hash.AppendData(
                buffer,
                0,
                read);
        }

        request.Body.Position = 0;

        return Convert.ToHexString(
                hash.GetHashAndReset())
            .ToLowerInvariant();
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int status,
        string title,
        string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType =
            "application/problem+json";

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            new ProblemDetailsPayload(
                status,
                title,
                detail),
            cancellationToken: context.RequestAborted);
    }

    private sealed record ProblemDetailsPayload(
        int Status,
        string Title,
        string Detail);
}
