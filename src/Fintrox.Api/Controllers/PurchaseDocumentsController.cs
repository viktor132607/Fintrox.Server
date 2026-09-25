using Fintrox.Application.Authorization;
using Fintrox.Application.Purchases;
using Fintrox.Contracts.Purchases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/purchases/documents")]
public sealed class PurchaseDocumentsController(
    IPurchaseDocumentService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.PurchasesRead)]
    public async Task<ActionResult<IReadOnlyList<PurchaseDocumentSummaryResponse>>> List(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] Guid? counterpartyId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListAsync(
                fromDate,
                toDate,
                status,
                type,
                counterpartyId,
                cancellationToken));
        }
        catch (PurchaseDocumentQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid purchase document query",
                exception.Message));
        }
    }

    [HttpGet("{documentId:guid}")]
    [Authorize(Policy = Permissions.PurchasesRead)]
    public async Task<ActionResult<PurchaseDocumentResponse>> Get(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await service.GetAsync(
            documentId,
            cancellationToken);

        return document is null ? NotFound() : Ok(document);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PurchasesWrite)]
    public async Task<ActionResult<PurchaseDocumentResponse>> Create(
        CreatePurchaseDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { documentId = document.Id },
                document);
        }
        catch (PurchaseDocumentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Purchase document conflict",
                exception.Message));
        }
        catch (PurchaseDocumentQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid purchase document",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid purchase document",
                exception.Message));
        }
    }

    [HttpPut("{documentId:guid}")]
    [Authorize(Policy = Permissions.PurchasesWrite)]
    public async Task<ActionResult<PurchaseDocumentResponse>> Update(
        Guid documentId,
        UpdatePurchaseDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await service.UpdateAsync(
                documentId,
                request,
                cancellationToken);

            return document is null ? NotFound() : Ok(document);
        }
        catch (PurchaseDocumentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Purchase document conflict",
                exception.Message));
        }
        catch (PurchaseDocumentQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid purchase document",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid purchase document",
                exception.Message));
        }
    }

    [HttpPost("{documentId:guid}/lines")]
    [Authorize(Policy = Permissions.PurchasesWrite)]
    public async Task<ActionResult<PurchaseDocumentLineResponse>> AddLine(
        Guid documentId,
        CreatePurchaseDocumentLineRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var line = await service.AddLineAsync(
                documentId,
                request,
                cancellationToken);

            return line is null
                ? NotFound()
                : StatusCode(StatusCodes.Status201Created, line);
        }
        catch (PurchaseDocumentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Purchase document conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid purchase document line",
                exception.Message));
        }
    }

    [HttpPut("{documentId:guid}/lines/{lineId:guid}")]
    [Authorize(Policy = Permissions.PurchasesWrite)]
    public async Task<ActionResult<PurchaseDocumentLineResponse>> UpdateLine(
        Guid documentId,
        Guid lineId,
        UpdatePurchaseDocumentLineRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var line = await service.UpdateLineAsync(
                documentId,
                lineId,
                request,
                cancellationToken);

            return line is null ? NotFound() : Ok(line);
        }
        catch (PurchaseDocumentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Purchase document conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid purchase document line",
                exception.Message));
        }
    }

    [HttpDelete("{documentId:guid}/lines/{lineId:guid}")]
    [Authorize(Policy = Permissions.PurchasesWrite)]
    public async Task<IActionResult> DeleteLine(
        Guid documentId,
        Guid lineId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.DeleteLineAsync(
                    documentId,
                    lineId,
                    cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (PurchaseDocumentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Purchase document conflict",
                exception.Message));
        }
    }

    [HttpPost("{documentId:guid}/receive")]
    [Authorize(Policy = Permissions.PurchasesWrite)]
    public async Task<ActionResult<PurchaseDocumentResponse>> Receive(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await service.ReceiveAsync(
                documentId,
                cancellationToken);

            return document is null ? NotFound() : Ok(document);
        }
        catch (PurchaseDocumentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Purchase document receive conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid purchase document",
                exception.Message));
        }
    }

    [HttpPost("{documentId:guid}/cancel")]
    [Authorize(Policy = Permissions.PurchasesWrite)]
    public async Task<ActionResult<PurchaseDocumentResponse>> Cancel(
        Guid documentId,
        CancelPurchaseDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await service.CancelAsync(
                documentId,
                request,
                cancellationToken);

            return document is null ? NotFound() : Ok(document);
        }
        catch (PurchaseDocumentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Purchase document cancellation conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid purchase document cancellation",
                exception.Message));
        }
    }

    private static ProblemDetails ToProblem(
        int status,
        string title,
        string detail) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };
}
