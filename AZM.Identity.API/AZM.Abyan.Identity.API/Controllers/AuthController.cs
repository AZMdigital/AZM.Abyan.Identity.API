using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace AZM.Abyan.Identity.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IUserService userService, IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IUserService _userService = userService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    //[HttpPost("logout")]
    //public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        await _authService.LogoutAsync(request, cancellationToken);
    //        return Ok(new { message = _localizer["OperationSuccess"] ?? "Operation completed successfully" });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
    //    }
    //}
    [HttpPost("logout")]
    public async Task<ActionResult> Logout(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await _authService.LogoutUserAsync(userId.ToString(), cancellationToken);
            return Ok(new { message = _localizer["OperationSuccess"] ?? "Operation completed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult> GetCurrentUserInfo(CancellationToken cancellationToken)
    {
        try
        {
            // ===== Get access token from Authorization header =====
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = _localizer["Unauthorized"] });
            }

            // ===== Try to get user ID from JWT claims =====
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            // If user ID not found, fallback: lookup by username
            if (string.IsNullOrEmpty(userId))
            {
                var username = User.FindFirst("preferred_username")?.Value
                               ?? User.FindFirst(ClaimTypes.Name)?.Value
                               ?? User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { message = _localizer["Unauthorized"] });

                var user = await _userService.GetUserByUsernameAsync(username, cancellationToken);
                if (user == null)
                    return NotFound(new { message = _localizer["UserNotFound"] });

                userId = user.Id;
            }

            // ===== Get full user info including Organizations from token =====
            var userInfo = await _userService.GetCurrentUserInfoAsync(userId, token, cancellationToken);

            if (userInfo == null)
                return NotFound(new { message = _localizer["UserNotFound"] });

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

}

