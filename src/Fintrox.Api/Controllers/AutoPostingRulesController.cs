using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Contracts.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/accounting/posting-rules")]
public sealed class AutoPostingRulesController(
    IAutoPostingRuleService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<IReadOnlyList<AutoPostingRuleResponse>>> List(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(includeInactive, cancellationToken));

    [HttpGet("{ruleId:guid}")]
    [Authorize(Policy = Permissions.AccountingRead)]
    public async Task<ActionResult<AutoPostingRuleResponse>> Get(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var rule = await service.GetAsync(ruleId, cancellationToken);
        return rule is null ? NotFound() : Ok(rule);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<AutoPostingRuleResponse>> Create(
        CreateAutoPostingRuleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { ruleId = rule.Id },
                rule);
        }
        catch (AutoPostingRuleConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Auto-posting rule conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid auto-posting rule",
                exception.Message));
        }
    }

    [HttpPut("{ruleId:guid}")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<AutoPostingRuleResponse>> Update(
        Guid ruleId,
        UpdateAutoPostingRuleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await service.UpdateAsync(
                ruleId,
                request,
                cancellationToken);

            return rule is null ? NotFound() : Ok(rule);
        }
        catch (AutoPostingRuleConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Auto-posting rule conflict",
                exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Invalid auto-posting rule",
                exception.Message));
        }
    }

    [HttpDelete("{ruleId:guid}")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<IActionResult> Deactivate(
        Guid ruleId,
        CancellationToken cancellationToken) =>
        await service.DeactivateAsync(ruleId, cancellationToken)
            ? NoContent()
            : NotFound();

    [HttpPost("{ruleId:guid}/activate")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    public async Task<ActionResult<AutoPostingRuleResponse>> Activate(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await service.ActivateAsync(
                ruleId,
                cancellationToken);

            return rule is null ? NotFound() : Ok(rule);
        }
        catch (AutoPostingRuleConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Auto-posting rule conflict",
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
