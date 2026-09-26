using Fintrox.Application.Authorization;
using Fintrox.Application.Integrations;
using Fintrox.Contracts.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize(Policy = Permissions.IntegrationsManage)]
[Route("api/v1/integrations/clients")]
public sealed class IntegrationClientsController(
    IIntegrationClientService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IntegrationClientResponse>>> List(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(
            includeInactive,
            cancellationToken));

    [HttpGet("{integrationClientId:guid}")]
    public async Task<ActionResult<IntegrationClientResponse>> Get(
        Guid integrationClientId,
        CancellationToken cancellationToken)
    {
        var client = await service.GetAsync(
            integrationClientId,
            cancellationToken);

        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<IntegrationClientSecretResponse>> Create(
        CreateIntegrationClientRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { integrationClientId = client.Client.Id },
                client);
        }
        catch (IntegrationClientConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Integration client conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid integration client",
                exception.Message));
        }
    }

    [HttpPut("{integrationClientId:guid}")]
    public async Task<ActionResult<IntegrationClientResponse>> Update(
        Guid integrationClientId,
        UpdateIntegrationClientRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await service.UpdateAsync(
                integrationClientId,
                request,
                cancellationToken);

            return client is null ? NotFound() : Ok(client);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid integration client",
                exception.Message));
        }
    }

    [HttpPost("{integrationClientId:guid}/rotate-secret")]
    public async Task<ActionResult<IntegrationClientSecretResponse>> RotateSecret(
        Guid integrationClientId,
        CancellationToken cancellationToken)
    {
        var response = await service.RotateSecretAsync(
            integrationClientId,
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpDelete("{integrationClientId:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid integrationClientId,
        CancellationToken cancellationToken) =>
        await service.DeactivateAsync(
            integrationClientId,
            cancellationToken)
            ? NoContent()
            : NotFound();

    [HttpPost("{integrationClientId:guid}/activate")]
    public async Task<ActionResult<IntegrationClientResponse>> Activate(
        Guid integrationClientId,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await service.ActivateAsync(
                integrationClientId,
                cancellationToken);

            return client is null ? NotFound() : Ok(client);
        }
        catch (IntegrationClientConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Integration client conflict",
                exception.Message));
        }
    }

    private static ProblemDetails ToProblem(
        int status,
        string title,
        string detail) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };
}
