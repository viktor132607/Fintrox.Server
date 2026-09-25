using Fintrox.Application.Authorization;
using Fintrox.Application.Reports;
using Fintrox.Contracts.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/reports")]
public sealed class AccountingReportsController(
    IAccountingReportService service) : ControllerBase
{
    [HttpGet("trial-balance")]
    [Authorize(Policy = Permissions.ReportsRead)]
    public async Task<ActionResult<TrialBalanceResponse>> TrialBalance(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] bool includeZeroBalances,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.GetTrialBalanceAsync(
                fromDate,
                toDate,
                includeZeroBalances,
                cancellationToken));
        }
        catch (AccountingReportQueryException exception)
        {
            return BadRequest(ToProblem(exception.Message));
        }
    }

    [HttpGet("general-ledger/{accountId:guid}")]
    [Authorize(Policy = Permissions.ReportsRead)]
    public async Task<ActionResult<GeneralLedgerResponse>> GeneralLedger(
        Guid accountId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await service.GetGeneralLedgerAsync(
                accountId,
                fromDate,
                toDate,
                cancellationToken);

            return report is null
                ? NotFound()
                : Ok(report);
        }
        catch (AccountingReportQueryException exception)
        {
            return BadRequest(ToProblem(exception.Message));
        }
    }

    private static ProblemDetails ToProblem(string detail)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid accounting report query",
            Detail = detail
        };
    }
}
