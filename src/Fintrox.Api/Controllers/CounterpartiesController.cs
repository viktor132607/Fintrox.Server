using Fintrox.Application.Authorization;
using Fintrox.Application.Counterparties;
using Fintrox.Contracts.Counterparties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/counterparties")]
public sealed class CounterpartiesController(
    ICounterpartyService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.CounterpartiesRead)]
    public async Task<ActionResult<IReadOnlyList<CounterpartyResponse>>> List(
        [FromQuery] bool includeInactive,
        [FromQuery] string? search,
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListAsync(
                includeInactive,
                search,
                role,
                cancellationToken));
        }
        catch (CounterpartyQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid counterparty query",
                exception.Message));
        }
    }

    [HttpGet("{counterpartyId:guid}")]
    [Authorize(Policy = Permissions.CounterpartiesRead)]
    public async Task<ActionResult<CounterpartyResponse>> Get(
        Guid counterpartyId,
        CancellationToken cancellationToken)
    {
        var counterparty = await service.GetAsync(
            counterpartyId,
            cancellationToken);

        return counterparty is null
            ? NotFound()
            : Ok(counterparty);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CounterpartiesWrite)]
    public async Task<ActionResult<CounterpartyResponse>> Create(
        CreateCounterpartyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var counterparty = await service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { counterpartyId = counterparty.Id },
                counterparty);
        }
        catch (CounterpartyConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Counterparty conflict",
                exception.Message));
        }
    }

    [HttpPut("{counterpartyId:guid}")]
    [Authorize(Policy = Permissions.CounterpartiesWrite)]
    public async Task<ActionResult<CounterpartyResponse>> Update(
        Guid counterpartyId,
        UpdateCounterpartyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var counterparty = await service.UpdateAsync(
                counterpartyId,
                request,
                cancellationToken);

            return counterparty is null
                ? NotFound()
                : Ok(counterparty);
        }
        catch (CounterpartyConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Counterparty conflict",
                exception.Message));
        }
    }

    [HttpDelete("{counterpartyId:guid}")]
    [Authorize(Policy = Permissions.CounterpartiesWrite)]
    public async Task<IActionResult> Deactivate(
        Guid counterpartyId,
        CancellationToken cancellationToken)
    {
        return await service.DeactivateAsync(
                counterpartyId,
                cancellationToken)
            ? NoContent()
            : NotFound();
    }

    [HttpPost("{counterpartyId:guid}/activate")]
    [Authorize(Policy = Permissions.CounterpartiesWrite)]
    public async Task<ActionResult<CounterpartyResponse>> Activate(
        Guid counterpartyId,
        CancellationToken cancellationToken)
    {
        var counterparty = await service.ActivateAsync(
            counterpartyId,
            cancellationToken);

        return counterparty is null
            ? NotFound()
            : Ok(counterparty);
    }

    private static ProblemDetails ToProblem(
        int status,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };
    }
}
