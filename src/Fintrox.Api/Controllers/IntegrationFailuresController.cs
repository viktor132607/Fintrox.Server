using Fintrox.Application.Authorization;
using Fintrox.Application.Integrations;
using Fintrox.Contracts.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize(Policy = Permissions.IntegrationsManage)]
[Route("api/v1/integrations/failures")]
public sealed class IntegrationFailuresController(
    IIntegrationFailureService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IntegrationFailureResponse>>> List(
        [FromQuery] bool includeResolved,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(
            includeResolved,
            cancellationToken));

    [HttpGet("{failureId:guid}")]
    public async Task<ActionResult<IntegrationFailureResponse>> Get(
        Guid failureId,
        CancellationToken cancellationToken)
    {
        var failure = await service.GetAsync(
            failureId,
            cancellationToken);

        return failure is null ? NotFound() : Ok(failure);
    }

    [HttpPost("{failureId:guid}/retry")]
    public async Task<ActionResult<IntegrationFailureResponse>> Retry(
        Guid failureId,
        CancellationToken cancellationToken)
    {
        var failure = await service.RetryAsync(
            failureId,
            cancellationToken);

        return failure is null ? NotFound() : Ok(failure);
    }
}
