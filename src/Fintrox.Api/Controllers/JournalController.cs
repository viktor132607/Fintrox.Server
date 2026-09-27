using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Contracts.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/accounting/journal")]
public sealed class JournalController(IJournalService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<IReadOnlyList<JournalEntrySummaryResponse>>> List(
        [FromQuery] DateOnly? fromPostingDate,
        [FromQuery] DateOnly? toPostingDate,
        [FromQuery] string? status,
        [FromQuery] string? source,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListAsync(
                fromPostingDate,
                toPostingDate,
                status,
                source,
                cancellationToken));
        }
        catch (JournalQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid journal query",
                exception.Message));
        }
    }

    [HttpGet("{journalEntryId:guid}")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<JournalEntryResponse>> Get(
        Guid journalEntryId,
        CancellationToken cancellationToken)
    {
        var entry = await service.GetAsync(
            journalEntryId,
            cancellationToken);

        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<JournalEntryResponse>> Create(
        CreateJournalEntryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var entry = await service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { journalEntryId = entry.Id },
                entry);
        }
        catch (JournalConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Journal conflict",
                exception.Message));
        }
    }

    [HttpPut("{journalEntryId:guid}")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<JournalEntryResponse>> Update(
        Guid journalEntryId,
        UpdateJournalEntryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var entry = await service.UpdateAsync(
                journalEntryId,
                request,
                cancellationToken);

            return entry is null ? NotFound() : Ok(entry);
        }
        catch (JournalConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Journal conflict",
                exception.Message));
        }
    }

    [HttpPost("{journalEntryId:guid}/lines")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<JournalLineResponse>> AddLine(
        Guid journalEntryId,
        CreateJournalLineRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var line = await service.AddLineAsync(
                journalEntryId,
                request,
                cancellationToken);

            return line is null
                ? NotFound()
                : StatusCode(StatusCodes.Status201Created, line);
        }
        catch (JournalConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Journal conflict",
                exception.Message));
        }
    }

    [HttpPut("{journalEntryId:guid}/lines/{journalLineId:guid}")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<JournalLineResponse>> UpdateLine(
        Guid journalEntryId,
        Guid journalLineId,
        UpdateJournalLineRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var line = await service.UpdateLineAsync(
                journalEntryId,
                journalLineId,
                request,
                cancellationToken);

            return line is null ? NotFound() : Ok(line);
        }
        catch (JournalConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Journal conflict",
                exception.Message));
        }
    }

    [HttpDelete("{journalEntryId:guid}/lines/{journalLineId:guid}")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<IActionResult> DeleteLine(
        Guid journalEntryId,
        Guid journalLineId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.DeleteLineAsync(
                    journalEntryId,
                    journalLineId,
                    cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (JournalConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Journal conflict",
                exception.Message));
        }
    }

    [HttpPost("{journalEntryId:guid}/post")]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<JournalEntryResponse>> Post(
        Guid journalEntryId,
        CancellationToken cancellationToken)
    {
        try
        {
            var entry = await service.PostAsync(
                journalEntryId,
                cancellationToken);

            return entry is null ? NotFound() : Ok(entry);
        }
        catch (JournalConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Journal posting conflict",
                exception.Message));
        }
    }

    [HttpPost("{journalEntryId:guid}/reverse")]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<JournalEntryResponse>> Reverse(
        Guid journalEntryId,
        ReverseJournalEntryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var reversal = await service.ReverseAsync(
                journalEntryId,
                request,
                cancellationToken);

            return reversal is null
                ? NotFound()
                : CreatedAtAction(
                    nameof(Get),
                    new { journalEntryId = reversal.Id },
                    reversal);
        }
        catch (JournalConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Journal reversal conflict",
                exception.Message));
        }
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
