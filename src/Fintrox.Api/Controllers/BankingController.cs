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

    [HttpPost("accounts/{accountId:guid}/statements/import-csv")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<BankStatementImportResponse>> ImportCsv(
        Guid accountId, ImportCsvBankStatementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ImportCsvStatementAsync(
                accountId, request, cancellationToken);
            if (result is null) return NotFound();
            return StatusCode(result.AlreadyImported
                ? StatusCodes.Status200OK : StatusCodes.Status201Created, result);
        }
        catch (BankReconciliationConflictException ex)
        {
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Bank CSV import conflict", ex.Message));
        }
    }

    [HttpGet("statements/{statementId:guid}/match-suggestions")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<BankMatchSuggestionsResponse>> MatchSuggestions(
        Guid statementId, CancellationToken cancellationToken)
    {
        var result = await service.GetMatchSuggestionsAsync(
            statementId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("statements/{statementId:guid}/auto-match")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<AutoMatchBankStatementResponse>> AutoMatch(
        Guid statementId, CancellationToken cancellationToken)
    {
        var result = await service.AutoMatchAsync(
            statementId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("statements/{statementId:guid}/foreign-close-evidence")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<ForeignBankCloseEvidenceResponse>> ForeignCloseEvidence(
        Guid statementId, CancellationToken cancellationToken)
    {
        var report = await service.GetForeignCloseEvidenceAsync(
            statementId, cancellationToken);
        return report is null ? NotFound() : Ok(report);
    }

    [HttpPost("statements/{statementId:guid}/prepare-foreign-close")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<ForeignBankCloseDecisionResponse>> PrepareForeignClose(
        Guid statementId, PrepareForeignBankCloseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var decision = await service.PrepareForeignCloseAsync(
                statementId, request, cancellationToken);
            return decision is null ? NotFound() : Ok(decision);
        }
        catch (BankReconciliationConflictException ex)
        {
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Foreign bank close preparation conflict", ex.Message));
        }
    }

    [HttpPost("statements/{statementId:guid}/approve-foreign-close")]
    [Authorize(Policy = Permissions.AccountingPost)]
    [Authorize(Policy = Permissions.OrganizationsManage)]
    public async Task<ActionResult<ForeignBankCloseDecisionResponse>> ApproveForeignClose(
        Guid statementId, ApproveForeignBankCloseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var decision = await service.ApproveForeignCloseAsync(
                statementId, request, cancellationToken);
            return decision is null ? NotFound() : Ok(decision);
        }
        catch (BankReconciliationConflictException ex)
        {
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Foreign bank close independent approval conflict", ex.Message));
        }
    }

    [HttpPost("statements/{statementId:guid}/revalue-foreign")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<ForeignBankRevaluationResponse>> RevalueForeign(
        Guid statementId, PostForeignBankRevaluationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.RevalueForeignBankAsync(
                statementId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (BankReconciliationConflictException ex)
        {
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Foreign bank period-end valuation conflict", ex.Message));
        }
    }

    [HttpPost("statements/{statementId:guid}/lines/{lineId:guid}/post-foreign-settlement")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<ForeignBankSettlementResponse>> PostForeignSettlement(
        Guid statementId, Guid lineId,
        PostForeignBankSettlementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PostForeignSettlementAsync(
                statementId, lineId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (BankReconciliationConflictException ex)
        {
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Foreign bank settlement conflict", ex.Message));
        }
    }

    [HttpPost("statements/{statementId:guid}/lines/{lineId:guid}/post-fee")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    public async Task<ActionResult<BankFeePostingResponse>> PostBankFee(
        Guid statementId,
        Guid lineId,
        PostBankFeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PostFeeAsync(
                statementId, lineId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (BankReconciliationConflictException ex)
        {
            return Conflict(Problem(StatusCodes.Status409Conflict,
                "Bank fee posting conflict", ex.Message));
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
