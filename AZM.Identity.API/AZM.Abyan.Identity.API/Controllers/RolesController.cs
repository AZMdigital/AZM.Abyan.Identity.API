using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.Role.Assign;
using AZM.Abyan.Identity.Application.Commands.Role.Create;
using AZM.Abyan.Identity.Application.Commands.Role.Delete;
using AZM.Abyan.Identity.Application.Commands.Role.Unassign;
using AZM.Abyan.Identity.Application.Commands.Role.Update;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Queries.Role.GetClientRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/[controller]")]
public class RolesController : BaseController
{

    [HttpGet("clients/{clientId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetClientRoles(string realm, Guid clientId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetClientRolesQuery(realm, clientId.ToString()), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("clients/{clientId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateClientRole(string realm, Guid clientId, [FromBody] CreateClientRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRoleCommand
        {
            Name = request.Name,
            Description = request.Description,
            Realm = realm,
            ClientId = clientId
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("clients/{clientId}/{roleName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateClientRole(string realm, Guid clientId, string roleName, [FromBody] UpdateClientRoleRequest request, CancellationToken cancellationToken)
    {
        // Get role by name to find the local ID
        var rolesResult = await Mediator.Send(new GetClientRolesQuery(realm, clientId.ToString()), cancellationToken);
        if (!rolesResult.IsSuccess || rolesResult.Data == null)
            return StatusCode(rolesResult.StatusCode, rolesResult);

        var role = rolesResult.Data.FirstOrDefault(r => r.Name == roleName);
        if (role == null || string.IsNullOrEmpty(role.Id) || !Guid.TryParse(role.Id, out var roleIdGuid))
            return NotFound(new { message = Localizer["RoleNotFound"] });

        var command = new UpdateRoleCommand
        {
            RoleId = roleIdGuid,
            UpdateRoleRequest = request,
            Realm = realm,
            KeycloakClientId = clientId.ToString(),
            RoleName = roleName // Original role name for Keycloak update
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("clients/{clientId}/{roleName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteClientRole(string realm, Guid clientId, string roleName, CancellationToken cancellationToken)
    {
        // Get role by name to find the local ID
        var rolesResult = await Mediator.Send(new GetClientRolesQuery(realm, clientId.ToString()), cancellationToken);
        if (!rolesResult.IsSuccess || rolesResult.Data == null)
            return StatusCode(rolesResult.StatusCode, rolesResult);

        var role = rolesResult.Data.FirstOrDefault(r => r.Name == roleName);
        if (role == null || string.IsNullOrEmpty(role.Id) || !Guid.TryParse(role.Id, out var roleIdGuid))
            return NotFound(new { message = "Role not found" });

        var command = new DeleteRoleCommand
        {
            RoleId = roleIdGuid,
            Realm = realm,
            KeycloakClientId = clientId.ToString(),
            RoleName = roleName
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AssignClientRoleToUser(string realm, Guid clientId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        request.ClientId = clientId.ToString();
        var command = new AssignClientRoleToUserCommand
        {
            AssignRoleRequest = request,
            Realm = realm
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("unassign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UnassignClientRoleFromUser(string realm, Guid clientId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        request.ClientId = clientId.ToString();
        var command = new RemoveClientRoleFromUserCommand
        {
            AssignRoleRequest = request,
            Realm = realm
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
