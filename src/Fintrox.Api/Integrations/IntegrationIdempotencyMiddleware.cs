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

        var metadata = TryCreateMetadata(
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
                        await next(context);

                        capture.Position = 0;
                        using var reader = new StreamReader(
                            capture,
                            Encoding.UTF8,
                            detectEncodingFromByteOrderMarks: true,
                            leaveOpen: true);

                        var body = await reader.ReadToEndAsync(
                            cancellationToken);

                        var resourceReference =
                            context.Response.Headers.Location.FirstOrDefault()
                            ?? context.Request.Path.Value;

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
                context.Response.Headers.Location =
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

    private static IntegrationIdempotencyRequest? TryCreateMetadata(
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
            IntegrationIdempotencyHeaders.SourceSystem);
        var externalId = ReadHeader(
            context,
            IntegrationIdempotencyHeaders.ExternalId);
        var eventType = ReadHeader(
            context,
            IntegrationIdempotencyHeaders.EventType);

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

        return new IntegrationIdempotencyRequest(
            organizationId.Value,
            integrationClientId,
            sourceSystem,
            externalId,
            eventType,
            context.Request.Method,
            requestPath,
            ComputeRequestHash(
                context.Request,
                requestPath));
    }

    private static string? ReadHeader(
        HttpContext context,
        string name)
    {
        var value = context.Request.Headers[name]
            .FirstOrDefault()?
            .Trim();

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }

    private static string ComputeRequestHash(
        HttpRequest request,
        string requestPath)
    {
        request.EnableBuffering();

        using var hash = IncrementalHash.CreateHash(
            HashAlgorithmName.SHA256);

        var prefix = Encoding.UTF8.GetBytes(
            $"{request.Method}\n{requestPath}\n");
        hash.AppendData(prefix);

        request.Body.Position = 0;

        Span<byte> buffer = stackalloc byte[8192];
        int read;

        while ((read = request.Body.Read(buffer)) > 0)
        {
            hash.AppendData(buffer[..read]);
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
