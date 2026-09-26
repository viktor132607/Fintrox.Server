using Fintrox.Application.Integrations;
using Fintrox.Contracts.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Route("api/v1/integrations")]
public sealed class IntegrationTokenController(
    IIntegrationClientService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<ActionResult<IntegrationTokenResponse>> Token(
        IntegrationTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ExchangeTokenAsync(
                request,
                cancellationToken));
        }
        catch (IntegrationAuthenticationException exception)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Integration authentication failed",
                Detail = exception.Message
            });
        }
        catch (IntegrationClientConflictException exception)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Integration authentication failed",
                Detail = exception.Message
            });
        }
    }
}
