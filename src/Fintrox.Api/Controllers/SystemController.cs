using Fintrox.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("info")]
    [ProducesResponseType<SystemInfoResponse>(StatusCodes.Status200OK)]
    public ActionResult<SystemInfoResponse> GetInfo()
    {
        return Ok(new SystemInfoResponse(
            Service: "Fintrox.Server",
            ApiVersion: "v1",
            RuntimeVersion: Environment.Version.ToString()));
    }
}
