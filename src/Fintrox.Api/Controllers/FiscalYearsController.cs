using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Contracts.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/accounting/fiscal-years")]
public sealed class FiscalYearsController(
    IFiscalCalendarService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<IReadOnlyList<FiscalYearResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListFiscalYearsAsync(cancellationToken));
    }

    [HttpGet("{fiscalYearId:guid}")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<FiscalYearResponse>> Get(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await service.GetFiscalYearAsync(
            fiscalYearId,
            cancellationToken);

        return fiscalYear is null ? NotFound() : Ok(fiscalYear);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<FiscalYearResponse>> Create(
        CreateFiscalYearRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fiscalYear = await service.CreateFiscalYearAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { fiscalYearId = fiscalYear.Id },
                fiscalYear);
        }
        catch (FiscalCalendarConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    [HttpPut("{fiscalYearId:guid}")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<FiscalYearResponse>> Update(
        Guid fiscalYearId,
        UpdateFiscalYearRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fiscalYear = await service.UpdateFiscalYearAsync(
                fiscalYearId,
                request,
                cancellationToken);

            return fiscalYear is null ? NotFound() : Ok(fiscalYear);
        }
        catch (FiscalCalendarConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    [HttpPost("{fiscalYearId:guid}/soft-close")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public Task<ActionResult<FiscalYearResponse>> SoftClose(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return ChangeFiscalYearStatusAsync(
            () => service.SoftCloseFiscalYearAsync(
                fiscalYearId,
                cancellationToken));
    }

    [HttpPost("{fiscalYearId:guid}/close")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public Task<ActionResult<FiscalYearResponse>> Close(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return ChangeFiscalYearStatusAsync(
            () => service.CloseFiscalYearAsync(
                fiscalYearId,
                cancellationToken));
    }

    [HttpPost("{fiscalYearId:guid}/reopen")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public Task<ActionResult<FiscalYearResponse>> Reopen(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return ChangeFiscalYearStatusAsync(
            () => service.ReopenFiscalYearAsync(
                fiscalYearId,
                cancellationToken));
    }

    [HttpGet("{fiscalYearId:guid}/periods")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<IReadOnlyList<AccountingPeriodResponse>>> ListPeriods(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListPeriodsAsync(
            fiscalYearId,
            cancellationToken));
    }

    [HttpGet("{fiscalYearId:guid}/periods/{periodId:guid}")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<AccountingPeriodResponse>> GetPeriod(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        var period = await service.GetPeriodAsync(
            fiscalYearId,
            periodId,
            cancellationToken);

        return period is null ? NotFound() : Ok(period);
    }

    [HttpPost("{fiscalYearId:guid}/periods")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<AccountingPeriodResponse>> CreatePeriod(
        Guid fiscalYearId,
        CreateAccountingPeriodRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var period = await service.CreatePeriodAsync(
                fiscalYearId,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetPeriod),
                new
                {
                    fiscalYearId,
                    periodId = period.Id
                },
                period);
        }
        catch (FiscalCalendarConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    [HttpPut("{fiscalYearId:guid}/periods/{periodId:guid}")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<AccountingPeriodResponse>> UpdatePeriod(
        Guid fiscalYearId,
        Guid periodId,
        UpdateAccountingPeriodRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var period = await service.UpdatePeriodAsync(
                fiscalYearId,
                periodId,
                request,
                cancellationToken);

            return period is null ? NotFound() : Ok(period);
        }
        catch (FiscalCalendarConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    [HttpPost("{fiscalYearId:guid}/periods/{periodId:guid}/soft-close")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public Task<ActionResult<AccountingPeriodResponse>> SoftClosePeriod(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        return ChangePeriodStatusAsync(
            () => service.SoftClosePeriodAsync(
                fiscalYearId,
                periodId,
                cancellationToken));
    }

    [HttpPost("{fiscalYearId:guid}/periods/{periodId:guid}/close")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public Task<ActionResult<AccountingPeriodResponse>> ClosePeriod(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        return ChangePeriodStatusAsync(
            () => service.ClosePeriodAsync(
                fiscalYearId,
                periodId,
                cancellationToken));
    }

    [HttpPost("{fiscalYearId:guid}/periods/{periodId:guid}/reopen")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public Task<ActionResult<AccountingPeriodResponse>> ReopenPeriod(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        return ChangePeriodStatusAsync(
            () => service.ReopenPeriodAsync(
                fiscalYearId,
                periodId,
                cancellationToken));
    }

    private async Task<ActionResult<FiscalYearResponse>>
        ChangeFiscalYearStatusAsync(
            Func<Task<FiscalYearResponse?>> operation)
    {
        try
        {
            var result = await operation();
            return result is null ? NotFound() : Ok(result);
        }
        catch (FiscalCalendarConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    private async Task<ActionResult<AccountingPeriodResponse>>
        ChangePeriodStatusAsync(
            Func<Task<AccountingPeriodResponse?>> operation)
    {
        try
        {
            var result = await operation();
            return result is null ? NotFound() : Ok(result);
        }
        catch (FiscalCalendarConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    private static ProblemDetails ToProblem(string detail)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Fiscal calendar conflict",
            Detail = detail
        };
    }
}
