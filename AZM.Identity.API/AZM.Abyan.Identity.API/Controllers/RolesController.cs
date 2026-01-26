using AZM.Abyan.Identity.Application.Commands.Role.Assign;
using AZM.Abyan.Identity.Application.Commands.Role.Create;
using AZM.Abyan.Identity.Application.Commands.Role.Delete;
using AZM.Abyan.Identity.Application.Commands.Role.Unassign;
using AZM.Abyan.Identity.Application.Commands.Role.Update;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RolesController(IRoleService roleService, IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _roleService = roleService;
        _mediator = mediator;
        _localizer = localizer;
    }

    [HttpGet("clients/{clientId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ClientRoleResponse>>> GetClientRoles(string realm, string clientId, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleService.GetClientRolesAsync(realm, clientId, cancellationToken);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost("clients/{clientId}")]
    public async Task<ActionResult> CreateClientRole(string realm, string clientId, [FromBody] CreateClientRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // clientId parameter is the Keycloak client string ID (e.g., "formbuilder")
            CreateRoleCommand command = new CreateRoleCommand();
            command.Name = request.Name;
            command.Description = request.Description;
            command.Realm = realm;
            command.KeycloakClientId = clientId; // Keycloak client string ID (e.g., "formbuilder")
            
            var result = await _mediator.Send(command);
            var response = StatusCode(result.StatusCode, result);
            return response;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPut("clients/{clientId}/{roleName}")]
    public async Task<ActionResult> UpdateClientRole(string realm, string clientId, string roleName, [FromBody] UpdateClientRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Get role by name to find the local ID
            var roles = await _roleService.GetClientRolesAsync(realm, clientId, cancellationToken);
            var role = roles.FirstOrDefault(r => r.Name == roleName);
            
            if (role == null || string.IsNullOrEmpty(role.Id) || !Guid.TryParse(role.Id, out var roleIdGuid))
            {
                return NotFound(new { message = _localizer["RoleNotFound"] });
            }

            var command = new UpdateRoleCommand
            {
                RoleId = roleIdGuid,
                UpdateRoleRequest = request,
                Realm = realm,
                KeycloakClientId = clientId,
                RoleName = roleName // Original role name for Keycloak update
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpDelete("clients/{clientId}/{roleName}")]
    public async Task<ActionResult> DeleteClientRole(string realm, string clientId, string roleName, CancellationToken cancellationToken)
    {
        try
        {
            // Get role by name to find the local ID
            var roles = await _roleService.GetClientRolesAsync(realm, clientId, cancellationToken);
            var role = roles.FirstOrDefault(r => r.Name == roleName);
            
            if (role == null || string.IsNullOrEmpty(role.Id) || !Guid.TryParse(role.Id, out var roleIdGuid))
            {
                return NotFound(new { message = _localizer["RoleNotFound"] });
            }

            var command = new DeleteRoleCommand
            {
                RoleId = roleIdGuid,
                Realm = realm,
                KeycloakClientId = clientId,
                RoleName = roleName
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost("assign")]
    public async Task<ActionResult> AssignClientRoleToUser(string realm, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new AssignClientRoleToUserCommand
            {
                AssignRoleRequest = request,
                Realm = realm
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost("unassign")]
    public async Task<ActionResult> UnassignClientRoleFromUser(string realm, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new RemoveClientRoleFromUserCommand
            {
                AssignRoleRequest = request,
                Realm = realm
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }
}

