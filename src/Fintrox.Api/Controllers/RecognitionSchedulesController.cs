using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Contracts.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/accounting/recognition-schedules")]
public sealed class RecognitionSchedulesController(
    IRecognitionScheduleService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<IReadOnlyList<RecognitionScheduleResponse>>> List(
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(cancellationToken));

    [HttpGet("{scheduleId:guid}")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<RecognitionScheduleDetailResponse>> Get(
        Guid scheduleId, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(scheduleId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<RecognitionScheduleResponse>> Create(
        CreateRecognitionScheduleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get),
                new { scheduleId = created.Id }, created);
        }
        catch (JournalConflictException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Invalid recognition schedule", detail: ex.Message);
        }
    }

    [HttpPost("{scheduleId:guid}/generate")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<RecognitionGenerationResponse>> Generate(
        Guid scheduleId, GenerateRecognitionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GenerateAsync(
                scheduleId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (JournalConflictException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Recognition generation conflict", detail: ex.Message);
        }
    }
}
