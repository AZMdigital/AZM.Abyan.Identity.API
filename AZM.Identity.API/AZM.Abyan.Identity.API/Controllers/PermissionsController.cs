using AZM.Abyan.Identity.API.Controllers.Base;
using AZM.Abyan.Identity.Application.Commands.Permission.Assign;
using AZM.Abyan.Identity.Application.Commands.Permission.Create;
using AZM.Abyan.Identity.Application.Commands.Permission.Delete;
using AZM.Abyan.Identity.Application.Commands.Permission.Unassign;
using AZM.Abyan.Identity.Application.Commands.Permission.Update;
using AZM.Abyan.Identity.Application.DTOs.Permissions;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using AZM.Abyan.Identity.Application.Queries.Permission.GetPermissionById;
using AZM.Abyan.Identity.Application.Queries.Permission.GetPermissions;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/clients/{clientId}/[controller]")]
public class PermissionsController : BaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetPermissions(string realm, Guid clientId, CancellationToken cancellationToken)
    {
        var query = new GetPermissionsQuery { ClientId = clientId };
        var result = await Mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{permissionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPermissionById(string realm, Guid clientId, Guid permissionId, CancellationToken cancellationToken)
    {
        var query = new GetPermissionByIdQuery { PermissionId = permissionId };
        var result = await Mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreatePermission(string realm, Guid clientId, [FromBody] CreatePermissionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePermissionCommand
        {
            Name = request.Name,
            Description = request.Description,
            ScopeId = request.ScopeId,
            ResourceId = request.ResourceId,
            PolicyId = request.PolicyId,
            ClientId = clientId, 
            RealmName = realm,
            KeycloakClientId = clientId.ToString()
        };

        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{permissionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePermission(string realm, Guid clientId, Guid permissionId, [FromBody] UpdatePermissionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePermissionCommand
        {
            PermissionId = permissionId,
            UpdatePermissionRequest = request
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }

    [HttpDelete("{permissionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePermission(string realm, Guid clientId, Guid permissionId, CancellationToken cancellationToken)
    {
        var command = new DeletePermissionCommand { PermissionId = permissionId };
        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }

    [HttpPost("assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AssignClientPermissionsToUser(string realm, Guid clientId, [FromBody] AssignPermissionRequest request, CancellationToken cancellationToken)
    {
        request.ClientId = clientId.ToString();
        var command = new AssignClientPermissionToUserCommand
        {
            AssignPermissionRequest = request,
            Realm = realm,
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }

    [HttpPost("unassign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UnassignClientPermissionsFromUser(string realm, Guid clientId, [FromBody] AssignPermissionRequest request, CancellationToken cancellationToken)
    {
        request.ClientId = clientId.ToString();
        var command = new RemoveClientPermissionFromUserCommand
        {
            AssignPermissionRequest = request,
            Realm = realm
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return NoContent();
    }
}
