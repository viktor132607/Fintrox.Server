using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Contracts.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/banking")]
public sealed class BankingController(IBankReconciliationService service)
    : ControllerBase
{
    [HttpGet("accounts")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<IReadOnlyList<BankAccountResponse>>> ListAccounts(
        CancellationToken cancellationToken) =>
        Ok(await service.ListAccountsAsync(cancellationToken));

    [HttpGet("statements/{statementId:guid}")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<BankStatementResponse>> GetStatement(
        Guid statementId, CancellationToken cancellationToken)
    {
        var statement = await service.GetStatementAsync(
            statementId, cancellationToken);
        return statement is null ? NotFound() : Ok(statement);
    }

    [HttpPost("accounts")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<BankAccountResponse>> CreateAccount(
        CreateBankAccountRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await service.CreateAccountAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, account);
        }
        catch (BankReconciliationConflictException ex)
        {
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Bank account conflict", ex.Message));
        }
    }

    [HttpPost("accounts/{accountId:guid}/statements")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<BankStatementImportResponse>> ImportStatement(
        Guid accountId, ImportBankStatementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var statement = await service.ImportStatementAsync(
                accountId, request, cancellationToken);
            if (statement is null) return NotFound();
            return StatusCode(statement.AlreadyImported
                ? StatusCodes.Status200OK : StatusCodes.Status201Created, statement);
        }
        catch (BankReconciliationConflictException ex)
        {
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Bank statement conflict", ex.Message));
        }
    }

    [HttpPost("statements/{statementId:guid}/lines/{lineId:guid}/match")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<BankStatementMatchResponse>> Match(
        Guid statementId, Guid lineId, MatchBankStatementLineRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.MatchLineAsync(
                statementId, lineId, request.PaymentId, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (BankReconciliationConflictException ex)
        {
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Bank match conflict", ex.Message));
        }
    }

    [HttpGet("statements/{statementId:guid}/reconciliation")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<BankReconciliationResponse>> Reconcile(
        Guid statementId, CancellationToken cancellationToken)
    {
        var report = await service.ReconcileAsync(statementId, cancellationToken);
        return report is null ? NotFound() : Ok(report);
    }

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };
}
