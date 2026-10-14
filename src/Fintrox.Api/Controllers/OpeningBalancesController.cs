using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Contracts.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/accounting/opening-balances")]
public sealed class OpeningBalancesController(IJournalService journals) : ControllerBase
{
    [HttpPost("import")]
    [Authorize(Policy = Permissions.AccountingWrite)]
    [Authorize(Policy = Permissions.AccountingPost)]
    [ProducesResponseType<JournalEntryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<JournalEntryResponse>> Import(
        ImportOpeningBalancesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await journals.ImportOpeningBalancesAsync(request, cancellationToken));
        }
        catch (JournalConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Opening balance import conflict",
                Detail = exception.Message
            });
        }
    }
}
