using Fintrox.Application.Common;
using Fintrox.Application.Organizations;
using Fintrox.Contracts.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/members")]
public sealed class OrganizationMembersController(
    IOrganizationMemberService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrganizationMemberResponse>>> List(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListAsync(organizationId, cancellationToken));
        }
        catch (ForbiddenOperationException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    public async Task<ActionResult<OrganizationMemberResponse>> Add(
        Guid organizationId,
        AddOrganizationMemberRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var member = await service.AddAsync(
                organizationId,
                request,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, member);
        }
        catch (ForbiddenOperationException)
        {
            return Forbid();
        }
        catch (OrganizationMembershipConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Membership conflict",
                Detail = exception.Message
            });
        }
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<ActionResult<OrganizationMemberResponse>> ChangeRole(
        Guid organizationId,
        Guid userId,
        UpdateOrganizationMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var member = await service.ChangeRoleAsync(
                organizationId,
                userId,
                request,
                cancellationToken);

            return member is null ? NotFound() : Ok(member);
        }
        catch (ForbiddenOperationException)
        {
            return Forbid();
        }
        catch (OrganizationMembershipConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Membership conflict",
                Detail = exception.Message
            });
        }
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.DeactivateAsync(
                    organizationId,
                    userId,
                    cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ForbiddenOperationException)
        {
            return Forbid();
        }
        catch (OrganizationMembershipConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Membership conflict",
                Detail = exception.Message
            });
        }
    }
}
