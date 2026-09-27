using Fintrox.Application.Authorization;
using Fintrox.Application.Currencies;
using Fintrox.Contracts.Currencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/currencies")]
public sealed class CurrenciesController(
    ICurrencyService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.CurrenciesRead)]
    public async Task<ActionResult<IReadOnlyList<CurrencyResponse>>> List(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListCurrenciesAsync(
            includeInactive,
            cancellationToken));
    }

    [HttpGet("{currencyId:guid}")]
    [Authorize(Policy = Permissions.CurrenciesRead)]
    public async Task<ActionResult<CurrencyResponse>> Get(
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var currency = await service.GetCurrencyAsync(
            currencyId,
            cancellationToken);

        return currency is null ? NotFound() : Ok(currency);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CurrenciesWrite)]
    public async Task<ActionResult<CurrencyResponse>> Create(
        CreateCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currency = await service.CreateCurrencyAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { currencyId = currency.Id },
                currency);
        }
        catch (CurrencyConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Currency conflict",
                exception.Message));
        }
        catch (CurrencyQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid currency",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid currency",
                exception.Message));
        }
    }

    [HttpPut("{currencyId:guid}")]
    [Authorize(Policy = Permissions.CurrenciesWrite)]
    public async Task<ActionResult<CurrencyResponse>> Update(
        Guid currencyId,
        UpdateCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currency = await service.UpdateCurrencyAsync(
                currencyId,
                request,
                cancellationToken);

            return currency is null ? NotFound() : Ok(currency);
        }
        catch (CurrencyConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Currency conflict",
                exception.Message));
        }
        catch (CurrencyQueryException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid currency",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid currency",
                exception.Message));
        }
    }

    [HttpDelete("{currencyId:guid}")]
    [Authorize(Policy = Permissions.CurrenciesWrite)]
    public async Task<IActionResult> Deactivate(
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.DeactivateCurrencyAsync(
                    currencyId,
                    cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (CurrencyConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Currency conflict",
                exception.Message));
        }
    }

    [HttpPost("{currencyId:guid}/activate")]
    [Authorize(Policy = Permissions.CurrenciesWrite)]
    public async Task<ActionResult<CurrencyResponse>> Activate(
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var currency = await service.ActivateCurrencyAsync(
            currencyId,
            cancellationToken);

        return currency is null ? NotFound() : Ok(currency);
    }

    [HttpPost("{currencyId:guid}/set-base")]
    [Authorize(Policy = Permissions.CurrenciesWrite)]
    public async Task<ActionResult<CurrencyResponse>> SetBase(
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        try
        {
            var currency = await service.SetBaseCurrencyAsync(
                currencyId,
                cancellationToken);

            return currency is null ? NotFound() : Ok(currency);
        }
        catch (CurrencyConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Currency conflict",
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
