using Fintrox.Application.Organizations;
using Fintrox.Contracts.Organizations;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Route("api/v1/organizations")]
public sealed class OrganizationsController(IOrganizationService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrganizationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrganizationResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrganizationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationResponse>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var organization = await service.GetAsync(id, cancellationToken);

        return organization is null
            ? NotFound()
            : Ok(organization);
    }

    [HttpPost]
    [ProducesResponseType<OrganizationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrganizationResponse>> Create(
        [FromBody] CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var organization = await service.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { id = organization.Id },
                organization);
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
    [ProducesResponseType<OrganizationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationResponse>> Update(
        Guid id,
        [FromBody] UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var organization = await service.UpdateAsync(id, request, cancellationToken);

        return organization is null
            ? NotFound()
            : Ok(organization);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await service.DeactivateAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType<OrganizationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationResponse>> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var organization = await service.ActivateAsync(id, cancellationToken);

        return organization is null
            ? NotFound()
            : Ok(organization);
    }
}
