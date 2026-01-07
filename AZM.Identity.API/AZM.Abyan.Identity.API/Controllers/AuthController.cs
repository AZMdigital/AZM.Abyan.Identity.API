using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
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
            return BadRequest(new { message = ex.Message });
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
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _authService.LogoutAsync(request, cancellationToken);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
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
                    return Unauthorized(new { message = "User ID or username not found in token" });
                }

                // Get user by username to get their ID
                var user = await _userService.GetUserByUsernameAsync(username, cancellationToken);
                if (user == null)
                {
                    return NotFound(new { message = $"User with username '{username}' not found" });
                }

                userId = user.Id;
            }

            var userInfo = await _userService.GetCurrentUserInfoAsync(userId, cancellationToken);
            
            if (userInfo == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

