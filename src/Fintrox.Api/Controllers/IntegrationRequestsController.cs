using Fintrox.Application.Authorization;
using Fintrox.Application.Integrations;
using Fintrox.Contracts.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize(Policy = Permissions.IntegrationsManage)]
[Route("api/v1/integrations/requests")]
public sealed class IntegrationRequestsController(
    IIntegrationRequestService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IntegrationRequestResponse>>> List(
        [FromQuery] string? sourceSystem,
        [FromQuery] string? externalId,
        [FromQuery] string? eventType,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListAsync(
                sourceSystem,
                externalId,
                eventType,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid integration request query",
                Detail = exception.Message
            });
        }
    }

    [HttpGet("{requestId:guid}")]
    public async Task<ActionResult<IntegrationRequestResponse>> Get(
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var request = await service.GetAsync(
            requestId,
            cancellationToken);

        return request is null
            ? NotFound()
            : Ok(request);
    }
}
