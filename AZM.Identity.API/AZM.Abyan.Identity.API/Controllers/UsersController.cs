using AZM.Abyan.Identity.Application.Commands.User.Create;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMediator _mediator;

    public UsersController(IUserService userService, IMediator mediator)
    {
        _userService = userService;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Command handler will create user in Keycloak and save to database
            CreateUserCommand command = new CreateUserCommand();
            command.Username = request.Username;
            command.Email = request.Email;
            command.FirstName = request.FirstName;
            command.LastName = request.LastName;
            command.Password = request.Password;
            command.Enabled = request.Enabled;
            command.EmailVerified = request.EmailVerified;
            // Note: TenantId and Realm should be provided in the request or resolved from context
            // For now, leaving them optional/nullable as they might come from different sources
            
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.UpdateUserAsync(id, request, cancellationToken);
            return Ok(new { message = $"User {id} updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(string id, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.DeleteUserAsync(id, cancellationToken);
            return Ok(new { message = $"User {id} deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userService.GetUsersAsync(cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
                return NotFound(new { message = $"User with id {id} not found" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/enable")]
    public async Task<ActionResult> EnableUser(string id, [FromBody] bool enabled, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.EnableUserAsync(id, enabled, cancellationToken);
            return Ok(new { message = $"User {(enabled ? "enabled" : "disabled")} successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/reset-password")]
    public async Task<ActionResult> ResetUserPassword(string id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.ResetUserPasswordAsync(id, request, cancellationToken);
            return Ok(new { message = $"Password for user {id} reset successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/send-verify-email")]
    public async Task<ActionResult> SendVerifyEmail(string id, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.SendVerifyEmailAsync(id, cancellationToken);
            return Ok(new { message = $"Verification email sent to user {id} successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

