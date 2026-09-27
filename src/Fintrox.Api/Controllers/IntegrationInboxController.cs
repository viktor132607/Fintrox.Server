using Fintrox.Application.Authorization;
using Fintrox.Application.Integrations;
using Fintrox.Contracts.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize(Policy = Permissions.IntegrationsManage)]
[Route("api/v1/integrations/inbox")]
public sealed class IntegrationInboxController(
    IIntegrationInboxService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IntegrationInboxResponse>>> List(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListAsync(
                status,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                "Invalid integration inbox query",
                exception.Message));
        }
    }

    [HttpGet("{inboxId:guid}")]
    public async Task<ActionResult<IntegrationInboxResponse>> Get(
        Guid inboxId,
        CancellationToken cancellationToken)
    {
        var inbox = await service.GetAsync(
            inboxId,
            cancellationToken);

        return inbox is null ? NotFound() : Ok(inbox);
    }

    [HttpPost("{inboxId:guid}/retry")]
    public async Task<ActionResult<IntegrationInboxResponse>> Retry(
        Guid inboxId,
        CancellationToken cancellationToken)
    {
        var inbox = await service.RetryAsync(
            inboxId,
            cancellationToken);

        return inbox is null ? NotFound() : Ok(inbox);
    }

    private static ProblemDetails ToProblem(
        string title,
        string detail) =>
        new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = title,
            Detail = detail
        };
}
