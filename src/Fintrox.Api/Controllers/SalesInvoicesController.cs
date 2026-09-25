using Fintrox.Application.Authorization;
using Fintrox.Application.Sales;
using Fintrox.Contracts.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sales/invoices")]
public sealed class SalesInvoicesController(
    ISalesInvoiceService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.SalesRead)]
    public async Task<ActionResult<IReadOnlyList<SalesInvoiceSummaryResponse>>> List(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? status,
        [FromQuery] Guid? counterpartyId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListAsync(
                fromDate,
                toDate,
                status,
                counterpartyId,
                cancellationToken));
        }
        catch (SalesInvoiceQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid sales invoice query",
                exception.Message));
        }
    }

    [HttpGet("{invoiceId:guid}")]
    [Authorize(Policy = Permissions.SalesRead)]
    public async Task<ActionResult<SalesInvoiceResponse>> Get(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var invoice = await service.GetAsync(
            invoiceId,
            cancellationToken);

        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.SalesWrite)]
    public async Task<ActionResult<SalesInvoiceResponse>> Create(
        CreateSalesInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { invoiceId = invoice.Id },
                invoice);
        }
        catch (SalesInvoiceConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Sales invoice conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid sales invoice",
                exception.Message));
        }
    }

    [HttpPut("{invoiceId:guid}")]
    [Authorize(Policy = Permissions.SalesWrite)]
    public async Task<ActionResult<SalesInvoiceResponse>> Update(
        Guid invoiceId,
        UpdateSalesInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await service.UpdateAsync(
                invoiceId,
                request,
                cancellationToken);

            return invoice is null ? NotFound() : Ok(invoice);
        }
        catch (SalesInvoiceConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Sales invoice conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid sales invoice",
                exception.Message));
        }
    }

    [HttpPost("{invoiceId:guid}/lines")]
    [Authorize(Policy = Permissions.SalesWrite)]
    public async Task<ActionResult<SalesInvoiceLineResponse>> AddLine(
        Guid invoiceId,
        CreateSalesInvoiceLineRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var line = await service.AddLineAsync(
                invoiceId,
                request,
                cancellationToken);

            return line is null
                ? NotFound()
                : StatusCode(StatusCodes.Status201Created, line);
        }
        catch (SalesInvoiceConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Sales invoice conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid sales invoice line",
                exception.Message));
        }
    }

    [HttpPut("{invoiceId:guid}/lines/{lineId:guid}")]
    [Authorize(Policy = Permissions.SalesWrite)]
    public async Task<ActionResult<SalesInvoiceLineResponse>> UpdateLine(
        Guid invoiceId,
        Guid lineId,
        UpdateSalesInvoiceLineRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var line = await service.UpdateLineAsync(
                invoiceId,
                lineId,
                request,
                cancellationToken);

            return line is null ? NotFound() : Ok(line);
        }
        catch (SalesInvoiceConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Sales invoice conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid sales invoice line",
                exception.Message));
        }
    }

    [HttpDelete("{invoiceId:guid}/lines/{lineId:guid}")]
    [Authorize(Policy = Permissions.SalesWrite)]
    public async Task<IActionResult> DeleteLine(
        Guid invoiceId,
        Guid lineId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.DeleteLineAsync(
                    invoiceId,
                    lineId,
                    cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (SalesInvoiceConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Sales invoice conflict",
                exception.Message));
        }
    }

    [HttpPost("{invoiceId:guid}/issue")]
    [Authorize(Policy = Permissions.SalesWrite)]
    public async Task<ActionResult<SalesInvoiceResponse>> Issue(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await service.IssueAsync(
                invoiceId,
                cancellationToken);

            return invoice is null ? NotFound() : Ok(invoice);
        }
        catch (SalesInvoiceConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Sales invoice issue conflict",
                exception.Message));
        }
    }

    [HttpPost("{invoiceId:guid}/cancel")]
    [Authorize(Policy = Permissions.SalesWrite)]
    public async Task<ActionResult<SalesInvoiceResponse>> Cancel(
        Guid invoiceId,
        CancelSalesInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await service.CancelAsync(
                invoiceId,
                request,
                cancellationToken);

            return invoice is null ? NotFound() : Ok(invoice);
        }
        catch (SalesInvoiceConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Sales invoice cancellation conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid sales invoice cancellation",
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
