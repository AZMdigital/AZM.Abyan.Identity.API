using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.Auth.Login;
using AZM.Abyan.Identity.Application.Commands.Auth.Logout;
using AZM.Abyan.Identity.Application.Commands.Auth.RefreshToken;
using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.Queries.Auth.GetCurrentUserInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AZM.Abyan.Identity.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new LoginCommand(request), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RefreshTokenCommand(request), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Logout(Guid userId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new LogoutCommand(userId), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetCurrentUserInfo(CancellationToken cancellationToken)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        var username = User.FindFirst("preferred_username")?.Value
                       ?? User.FindFirst(ClaimTypes.Name)?.Value
                       ?? User.Identity?.Name;

        var query = new GetCurrentUserInfoQuery
        {
            AccessToken = token,
            UserId = userId,
            Username = username
        };

        var result = await Mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
