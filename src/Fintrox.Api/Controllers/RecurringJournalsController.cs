using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Contracts.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

/// <summary>
/// Monthly recurring templates are immutable; generation creates a reviewable
/// System draft, and posting uses the separately authorized GL posting route.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/accounting/recurring-journals")]
public sealed class RecurringJournalsController(IRecurringJournalService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<IReadOnlyList<RecurringJournalTemplateResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListAsync(cancellationToken));
    }

    [HttpGet("{templateId:guid}/schedule")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<RecurringJournalScheduleResponse>> Schedule(
        Guid templateId, [FromQuery] DateOnly from, [FromQuery] DateOnly to,
        [FromQuery] DateOnly? asOf, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetScheduleAsync(
                templateId, from, to, asOf, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (JournalConflictException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Invalid recurring journal schedule window",
                detail: ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<RecurringJournalTemplateResponse>> Create(
        CreateRecurringJournalTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (JournalConflictException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Invalid recurring journal template",
                detail: ex.Message);
        }
    }

    [HttpGet("{templateId:guid}/versions")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<RecurringJournalLineageResponse>> Versions(
        Guid templateId, CancellationToken cancellationToken)
    {
        return await service.GetLineageAsync(templateId, cancellationToken) is { } result
            ? Ok(result) : NotFound();
    }

    [HttpPost("{templateId:guid}/revise")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<RecurringJournalRevisionResponse>> Revise(
        Guid templateId, ReviseRecurringJournalTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ReviseAsync(templateId, request, cancellationToken);
            return result is null ? NotFound() :
                StatusCode(StatusCodes.Status201Created, result);
        }
        catch (JournalConflictException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Recurring template revision conflict",
                detail: ex.Message);
        }
    }

    [HttpPost("{templateId:guid}/generate")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<RecurringJournalGenerationResponse>> Generate(
        Guid templateId, GenerateRecurringJournalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GenerateAsync(
                templateId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (JournalConflictException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Recurring journal generation conflict",
                detail: ex.Message);
        }
    }
}
