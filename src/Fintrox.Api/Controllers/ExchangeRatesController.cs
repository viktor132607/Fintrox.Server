using Fintrox.Application.Authorization;
using Fintrox.Application.Currencies;
using Fintrox.Contracts.Currencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/exchange-rates")]
public sealed class ExchangeRatesController(
    ICurrencyService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.CurrenciesRead)]
    public async Task<ActionResult<IReadOnlyList<ExchangeRateResponse>>> List(
        [FromQuery] Guid? baseCurrencyId,
        [FromQuery] Guid? quoteCurrencyId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListExchangeRatesAsync(
                baseCurrencyId,
                quoteCurrencyId,
                fromDate,
                toDate,
                cancellationToken));
        }
        catch (CurrencyQueryException exception)
        {
            return BadRequest(ToProblem(exception.Message));
        }
    }

    [HttpGet("{exchangeRateId:guid}")]
    [Authorize(Policy = Permissions.CurrenciesRead)]
    public async Task<ActionResult<ExchangeRateResponse>> Get(
        Guid exchangeRateId,
        CancellationToken cancellationToken)
    {
        var rate = await service.GetExchangeRateAsync(
            exchangeRateId,
            cancellationToken);

        return rate is null ? NotFound() : Ok(rate);
    }

    [HttpGet("latest")]
    [Authorize(Policy = Permissions.CurrenciesRead)]
    public async Task<ActionResult<ExchangeRateResponse>> Latest(
        [FromQuery] Guid baseCurrencyId,
        [FromQuery] Guid quoteCurrencyId,
        [FromQuery] DateOnly? asOfDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var rate = await service.GetLatestExchangeRateAsync(
                baseCurrencyId,
                quoteCurrencyId,
                asOfDate,
                cancellationToken);

            return rate is null ? NotFound() : Ok(rate);
        }
        catch (CurrencyQueryException exception)
        {
            return BadRequest(ToProblem(exception.Message));
        }
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CurrenciesWrite)]
    public async Task<ActionResult<ExchangeRateResponse>> Create(
        CreateExchangeRateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var rate = await service.CreateExchangeRateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { exchangeRateId = rate.Id },
                rate);
        }
        catch (CurrencyConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
        catch (CurrencyQueryException exception)
        {
            return BadRequest(ToProblem(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(exception.Message));
        }
    }

    [HttpPut("{exchangeRateId:guid}")]
    [Authorize(Policy = Permissions.CurrenciesWrite)]
    public async Task<ActionResult<ExchangeRateResponse>> Update(
        Guid exchangeRateId,
        UpdateExchangeRateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var rate = await service.UpdateExchangeRateAsync(
                exchangeRateId,
                request,
                cancellationToken);

            return rate is null ? NotFound() : Ok(rate);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(exception.Message));
        }
    }

    private static ProblemDetails ToProblem(string detail) =>
        new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid exchange rate",
            Detail = detail
        };
}
