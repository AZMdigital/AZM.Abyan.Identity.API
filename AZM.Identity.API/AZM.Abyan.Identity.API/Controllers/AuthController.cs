using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AuthController(IAuthService authService, IUserService userService, IStringLocalizer<SharedResource> localizer)
    {
        _authService = authService;
        _userService = userService;
        _localizer = localizer;
    }

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
    public async Task<ActionResult> Logout(string userId, CancellationToken cancellationToken)
    {
        try
        {
            await _authService.LogoutUserAsync(userId, cancellationToken);
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
            // Try to get user ID from JWT token claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;

            // If user ID not found, try to get username and look up user
            if (string.IsNullOrEmpty(userId))
            {
                var username = User.FindFirst("preferred_username")?.Value
                             ?? User.FindFirst(ClaimTypes.Name)?.Value
                             ?? User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(new { message = _localizer["Unauthorized"] });
                }

                // Get user by username to get their ID
                var user = await _userService.GetUserByUsernameAsync(username, cancellationToken);
                if (user == null)
                {
                    return NotFound(new { message = _localizer["UserNotFound"] });
                }

                userId = user.Id;
            }

            var userInfo = await _userService.GetCurrentUserInfoAsync(userId, cancellationToken);
            
            if (userInfo == null)
            {
                return NotFound(new { message = _localizer["UserNotFound"] });
            }

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }
}

