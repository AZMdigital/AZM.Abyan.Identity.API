using AZM.Abyan.Identity.Application.Commands.User.Create;
using AZM.Abyan.Identity.Application.Commands.User.Delete;
using AZM.Abyan.Identity.Application.Commands.User.Update;
using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UsersController(IUserService userService, IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _userService = userService;
        _mediator = mediator;
        _localizer = localizer;
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
            command.OrganizationName = request.OrganizationName; // Set RealmName from request
            
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.UserId = id;
            var result = await _mediator.Send(new UpdateUserCommand(request));
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(string id, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(id, out var userId))
            {
                return BadRequest(new { message = _localizer["InvalidUserId"] });
            }
            var result = await _mediator.Send(new DeleteUserCommand(userId));
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
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
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
                return NotFound(new { message = _localizer["UserNotFound"] });

            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    //[HttpPut("{id}/enable")]
    //public async Task<ActionResult> EnableUser(string id, [FromBody] bool enabled, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        await _userService.EnableUserAsync(id, enabled, cancellationToken);
    //        return Ok(new { message = $"User {(enabled ? "enabled" : "disabled")} successfully" });
    //    }
    //    catch (KeyNotFoundException ex)
    //    {
    //        return NotFound(new { message = ex.Message });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var sent = await _userService.ForgotPasswordAsync(request, cancellationToken);

            if (!sent)
            {
                return NotFound(new { message = _localizer["UserNotFound"] });
            }

            return Ok(new { message = _localizer["OperationSuccess"] ?? "Reset password email sent" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
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

