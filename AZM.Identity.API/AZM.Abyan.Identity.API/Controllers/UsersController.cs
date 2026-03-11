using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.User.Create;
using AZM.Abyan.Identity.Application.Commands.User.Delete;
using AZM.Abyan.Identity.Application.Commands.User.ForgotPassword;
using AZM.Abyan.Identity.Application.Commands.User.ResetPassword;
using AZM.Abyan.Identity.Application.Commands.User.SendVerifyEmail;
using AZM.Abyan.Identity.Application.Commands.User.Update;
using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Queries.User.GetUserById;
using AZM.Abyan.Identity.Application.Queries.User.GetUsers;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Command handler will create user in Keycloak and save to database
        CreateUserCommand command = new CreateUserCommand
        {
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = request.Password,
            Enabled = request.Enabled,
            EmailVerified = request.EmailVerified,
            OrganizationName = request.OrganizationName
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        request.UserId = id;
        var result = await Mediator.Send(new UpdateUserCommand(request), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(string id, CancellationToken cancellationToken)
    {
      
        var result = await Mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUsersQuery(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUserById(string id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ForgotPasswordCommand(request), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetUserPassword(string id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ResetUserPasswordCommand(id, request), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}/send-verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SendVerifyEmail(string id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SendVerifyEmailCommand(id), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

