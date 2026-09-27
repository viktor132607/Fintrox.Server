using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Contracts.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/accounting/accounts")]
public sealed class AccountsController(IAccountService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<IReadOnlyList<AccountResponse>>> List(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListAsync(
            includeInactive,
            cancellationToken));
    }

    [HttpGet("tree")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<IReadOnlyList<AccountTreeNodeResponse>>> Tree(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        return Ok(await service.GetTreeAsync(
            includeInactive,
            cancellationToken));
    }

    [HttpGet("{accountId:guid}")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<AccountResponse>> Get(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var account = await service.GetAsync(
            accountId,
            cancellationToken);

        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<AccountResponse>> Create(
        CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { accountId = account.Id },
                account);
        }
        catch (AccountConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    [HttpPut("{accountId:guid}")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<AccountResponse>> Update(
        Guid accountId,
        UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await service.UpdateAsync(
                accountId,
                request,
                cancellationToken);

            return account is null ? NotFound() : Ok(account);
        }
        catch (AccountConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    [HttpDelete("{accountId:guid}")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<IActionResult> Deactivate(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.DeactivateAsync(
                    accountId,
                    cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (AccountConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    [HttpPost("{accountId:guid}/activate")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<AccountResponse>> Activate(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await service.ActivateAsync(
                accountId,
                cancellationToken);

            return account is null ? NotFound() : Ok(account);
        }
        catch (AccountConflictException exception)
        {
            return Conflict(ToProblem(exception.Message));
        }
    }

    private static ProblemDetails ToProblem(string detail)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Chart of accounts conflict",
            Detail = detail
        };
    }
}
