using Fintrox.Application.Authorization;
using Fintrox.Application.Payments;
using Fintrox.Contracts.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/payments")]
public sealed class PaymentsController(
    IPaymentService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.PaymentsRead)]
    public async Task<ActionResult<IReadOnlyList<PaymentSummaryResponse>>> List(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? status,
        [FromQuery] string? direction,
        [FromQuery] Guid? counterpartyId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListAsync(
                fromDate,
                toDate,
                status,
                direction,
                counterpartyId,
                cancellationToken));
        }
        catch (PaymentQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid payment query",
                exception.Message));
        }
    }

    [HttpGet("{paymentId:guid}")]
    [Authorize(Policy = Permissions.PaymentsRead)]
    public async Task<ActionResult<PaymentResponse>> Get(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await service.GetAsync(paymentId, cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PaymentsWrite)]
    public async Task<ActionResult<PaymentResponse>> Create(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { paymentId = payment.Id }, payment);
        }
        catch (PaymentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Payment conflict",
                exception.Message));
        }
        catch (PaymentQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid payment",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid payment",
                exception.Message));
        }
    }

    [HttpPut("{paymentId:guid}")]
    [Authorize(Policy = Permissions.PaymentsWrite)]
    public async Task<ActionResult<PaymentResponse>> Update(
        Guid paymentId,
        UpdatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await service.UpdateAsync(
                paymentId,
                request,
                cancellationToken);

            return payment is null ? NotFound() : Ok(payment);
        }
        catch (PaymentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Payment conflict",
                exception.Message));
        }
        catch (PaymentQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid payment",
                exception.Message));
        }
    }

    [HttpPost("{paymentId:guid}/allocations")]
    [Authorize(Policy = Permissions.PaymentsWrite)]
    public async Task<ActionResult<PaymentAllocationResponse>> AddAllocation(
        Guid paymentId,
        CreatePaymentAllocationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var allocation = await service.AddAllocationAsync(
                paymentId,
                request,
                cancellationToken);

            return allocation is null
                ? NotFound()
                : StatusCode(StatusCodes.Status201Created, allocation);
        }
        catch (PaymentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Payment allocation conflict",
                exception.Message));
        }
        catch (PaymentQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid payment allocation",
                exception.Message));
        }
    }

    [HttpPut("{paymentId:guid}/allocations/{allocationId:guid}")]
    [Authorize(Policy = Permissions.PaymentsWrite)]
    public async Task<ActionResult<PaymentAllocationResponse>> UpdateAllocation(
        Guid paymentId,
        Guid allocationId,
        UpdatePaymentAllocationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var allocation = await service.UpdateAllocationAsync(
                paymentId,
                allocationId,
                request,
                cancellationToken);

            return allocation is null ? NotFound() : Ok(allocation);
        }
        catch (PaymentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Payment allocation conflict",
                exception.Message));
        }
        catch (PaymentQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid payment allocation",
                exception.Message));
        }
    }

    [HttpDelete("{paymentId:guid}/allocations/{allocationId:guid}")]
    [Authorize(Policy = Permissions.PaymentsWrite)]
    public async Task<IActionResult> DeleteAllocation(
        Guid paymentId,
        Guid allocationId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.DeleteAllocationAsync(
                    paymentId,
                    allocationId,
                    cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (PaymentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Payment allocation conflict",
                exception.Message));
        }
    }

    [HttpPost("{paymentId:guid}/confirm")]
    [Authorize(Policy = Permissions.PaymentsWrite)]
    public async Task<ActionResult<PaymentResponse>> Confirm(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await service.ConfirmAsync(
                paymentId,
                cancellationToken);

            return payment is null ? NotFound() : Ok(payment);
        }
        catch (PaymentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Payment confirmation conflict",
                exception.Message));
        }
    }

    [HttpPost("{paymentId:guid}/cancel")]
    [Authorize(Policy = Permissions.PaymentsWrite)]
    public async Task<ActionResult<PaymentResponse>> Cancel(
        Guid paymentId,
        CancelPaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await service.CancelAsync(
                paymentId,
                request,
                cancellationToken);

            return payment is null ? NotFound() : Ok(payment);
        }
        catch (PaymentConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Payment cancellation conflict",
                exception.Message));
        }
    }

    [HttpGet("settlements/sales-invoices/{invoiceId:guid}")]
    [Authorize(Policy = Permissions.PaymentsRead)]
    public async Task<ActionResult<DocumentSettlementResponse>> SalesSettlement(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var settlement = await service.GetSalesInvoiceSettlementAsync(
            invoiceId,
            cancellationToken);

        return settlement is null ? NotFound() : Ok(settlement);
    }

    [HttpGet("settlements/purchase-documents/{documentId:guid}")]
    [Authorize(Policy = Permissions.PaymentsRead)]
    public async Task<ActionResult<DocumentSettlementResponse>> PurchaseSettlement(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var settlement = await service.GetPurchaseDocumentSettlementAsync(
            documentId,
            cancellationToken);

        return settlement is null ? NotFound() : Ok(settlement);
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
