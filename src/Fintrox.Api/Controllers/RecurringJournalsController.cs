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
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Invalid recurring journal template", ex.Message));
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
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Recurring journal generation conflict", ex.Message));
        }
    }
}
