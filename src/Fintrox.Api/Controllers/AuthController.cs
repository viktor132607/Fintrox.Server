using Fintrox.Application.Common.Interfaces;
using Fintrox.Application.Identity;
using Fintrox.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    IAuthenticationService authenticationService,
    ICurrentUser currentUser) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<AuthTokenResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthTokenResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await authenticationService.RegisterAsync(
                request,
                GetIpAddress(),
                GetUserAgent(),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (IdentityConflictException exception)
        {
            return Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Registration failed",
                exception.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authenticationService.LoginAsync(
                request,
                GetIpAddress(),
                GetUserAgent(),
                cancellationToken));
        }
        catch (AuthenticationException exception)
        {
            return Unauthorized(ToProblem(
                StatusCodes.Status401Unauthorized,
                "Authentication failed",
                exception.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType<AuthTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authenticationService.RefreshAsync(
                request,
                GetIpAddress(),
                GetUserAgent(),
                cancellationToken));
        }
        catch (AuthenticationException exception)
        {
            return Unauthorized(ToProblem(
                StatusCodes.Status401Unauthorized,
                "Token refresh failed",
                exception.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke(
        RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await authenticationService.RevokeAsync(
            request,
            GetIpAddress(),
            cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentUserResponse>> Me(
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authenticationService.GetCurrentUserAsync(
                currentUser.RequireUserId(),
                cancellationToken));
        }
        catch (AuthenticationException exception)
        {
            return Unauthorized(ToProblem(
                StatusCodes.Status401Unauthorized,
                "Authentication failed",
                exception.Message));
        }
    }

    [Authorize]
    [HttpGet("sessions")]
    [ProducesResponseType<IReadOnlyList<SessionResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> Sessions(
        CancellationToken cancellationToken)
    {
        return Ok(await authenticationService.ListSessionsAsync(
            currentUser.RequireUserId(),
            cancellationToken));
    }

    [Authorize]
    [HttpDelete("sessions/{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var revoked = await authenticationService.RevokeSessionAsync(
            currentUser.RequireUserId(),
            sessionId,
            GetIpAddress(),
            cancellationToken);

        return revoked ? NoContent() : NotFound();
    }

    private string? GetIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? GetUserAgent() =>
        Request.Headers["User-Agent"].FirstOrDefault();

    private static ProblemDetails ToProblem(
        int status,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };
    }
}
