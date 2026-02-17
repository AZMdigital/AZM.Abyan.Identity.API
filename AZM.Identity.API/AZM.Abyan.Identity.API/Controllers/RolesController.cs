using AZM.Abyan.Identity.Application.Commands.Role.Assign;
using AZM.Abyan.Identity.Application.Commands.Role.Create;
using AZM.Abyan.Identity.Application.Commands.Role.Delete;
using AZM.Abyan.Identity.Application.Commands.Role.Unassign;
using AZM.Abyan.Identity.Application.Commands.Role.Update;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class RolesController(IRoleService roleService, IMediator mediator, IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;
    private readonly IMediator _mediator = mediator;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    [HttpGet("clients/{clientId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ClientRoleResponse>>> GetClientRoles(string realm, Guid clientId, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleService.GetClientRolesAsync(realm, clientId.ToString(), cancellationToken);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost("clients/{clientId}")]
    public async Task<ActionResult> CreateClientRole(string realm, Guid clientId, [FromBody] CreateClientRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Command handler will create role in Keycloak and save to database
            // clientId parameter is the Keycloak client ID (string), we need to parse it to Guid for local ClientId
            if (!Guid.TryParse(clientId, out var clientIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidClientId"] });
            }

            CreateRoleCommand command = new CreateRoleCommand();
            command.Name = request.Name;
            command.Description = request.Description;
            command.Realm = realm;
            command.ClientId = clientId; // Keycloak client string ID (e.g., "formbuilder")
            
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPut("clients/{clientId}/{roleName}")]
    public async Task<ActionResult> UpdateClientRole(string realm, Guid clientId, string roleName, [FromBody] UpdateClientRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Get role by name to find the local ID
            var roles = await _roleService.GetClientRolesAsync(realm, clientId.ToString(), cancellationToken);
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
                KeycloakClientId = clientId.ToString(),
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
    public async Task<ActionResult> DeleteClientRole(string realm, Guid clientId, string roleName, CancellationToken cancellationToken)
    {
        try
        {
            // Get role by name to find the local ID
            var roles = await _roleService.GetClientRolesAsync(realm, clientId.ToString(), cancellationToken);
            var role = roles.FirstOrDefault(r => r.Name == roleName);

            if (role == null || string.IsNullOrEmpty(role.Id) || !Guid.TryParse(role.Id, out var roleIdGuid))
            {
                return NotFound(new { message = _localizer["RoleNotFound"] });
            }

            var command = new DeleteRoleCommand
            {
                RoleId = roleIdGuid,
                Realm = realm,
                KeycloakClientId = clientId.ToString(),
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
    public async Task<ActionResult> AssignClientRoleToUser(string realm, Guid clientId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.ClientId = clientId.ToString();
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
    public async Task<ActionResult> UnassignClientRoleFromUser(string realm, Guid clientId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.ClientId = clientId.ToString();
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

