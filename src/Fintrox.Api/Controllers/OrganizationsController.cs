using Fintrox.Application.Common;
using Fintrox.Application.Organizations;
using Fintrox.Contracts.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/organizations")]
public sealed class OrganizationsController(IOrganizationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrganizationResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationResponse>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var organization = await service.GetAsync(id, cancellationToken);
        return organization is null ? NotFound() : Ok(organization);
    }

    [HttpPost]
    public async Task<ActionResult<OrganizationResponse>> Create(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var organization = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = organization.Id }, organization);
        }
        catch (OrganizationConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Organization conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrganizationResponse>> Update(
        Guid id,
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var organization = await service.UpdateAsync(id, request, cancellationToken);
            return organization is null ? NotFound() : Ok(organization);
        }
        catch (ForbiddenOperationException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.DeactivateAsync(id, cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ForbiddenOperationException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<OrganizationResponse>> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var organization = await service.ActivateAsync(id, cancellationToken);
            return organization is null ? NotFound() : Ok(organization);
        }
        catch (ForbiddenOperationException)
        {
            return Forbid();
        }
    }
}
