using Fintrox.Application.Authorization;
using Fintrox.Application.Tax;
using Fintrox.Contracts.Tax;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tax/vat-codes")]
public sealed class VatCodesController(
    IVatCodeService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.TaxRead)]
    public async Task<ActionResult<IReadOnlyList<VatCodeResponse>>> List(
        [FromQuery] bool includeInactive,
        [FromQuery] DateOnly? asOfDate,
        [FromQuery] string? appliesTo,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListAsync(
                includeInactive,
                asOfDate,
                appliesTo,
                cancellationToken));
        }
        catch (VatCodeQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid VAT code query",
                exception.Message));
        }
    }

    [HttpGet("{vatCodeId:guid}")]
    [Authorize(Policy = Permissions.TaxRead)]
    public async Task<ActionResult<VatCodeResponse>> Get(
        Guid vatCodeId,
        CancellationToken cancellationToken)
    {
        var vatCode = await service.GetAsync(vatCodeId, cancellationToken);
        return vatCode is null ? NotFound() : Ok(vatCode);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.TaxWrite)]
    public async Task<ActionResult<VatCodeResponse>> Create(
        CreateVatCodeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vatCode = await service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { vatCodeId = vatCode.Id },
                vatCode);
        }
        catch (VatCodeConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "VAT code conflict",
                exception.Message));
        }
        catch (VatCodeQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid VAT code",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid VAT code",
                exception.Message));
        }
    }

    [HttpPut("{vatCodeId:guid}")]
    [Authorize(Policy = Permissions.TaxWrite)]
    public async Task<ActionResult<VatCodeResponse>> Update(
        Guid vatCodeId,
        UpdateVatCodeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vatCode = await service.UpdateAsync(
                vatCodeId,
                request,
                cancellationToken);

            return vatCode is null ? NotFound() : Ok(vatCode);
        }
        catch (VatCodeConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "VAT code conflict",
                exception.Message));
        }
        catch (VatCodeQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid VAT code",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid VAT code",
                exception.Message));
        }
    }

    [HttpDelete("{vatCodeId:guid}")]
    [Authorize(Policy = Permissions.TaxWrite)]
    public async Task<IActionResult> Deactivate(
        Guid vatCodeId,
        CancellationToken cancellationToken)
    {
        return await service.DeactivateAsync(
                vatCodeId,
                cancellationToken)
            ? NoContent()
            : NotFound();
    }

    [HttpPost("{vatCodeId:guid}/activate")]
    [Authorize(Policy = Permissions.TaxWrite)]
    public async Task<ActionResult<VatCodeResponse>> Activate(
        Guid vatCodeId,
        CancellationToken cancellationToken)
    {
        var vatCode = await service.ActivateAsync(
            vatCodeId,
            cancellationToken);

        return vatCode is null ? NotFound() : Ok(vatCode);
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
