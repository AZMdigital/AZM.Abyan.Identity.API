using AZM.Abyan.Identity.Application.Commands.Permission.Create;
using AZM.Abyan.Identity.Application.Commands.Permission.Delete;
using AZM.Abyan.Identity.Application.Commands.Permission.Update;
using AZM.Abyan.Identity.Application.DTOs.Permissions;
using AZM.Abyan.Identity.Application.Queries.Permission.GetPermissionById;
using AZM.Abyan.Identity.Application.Queries.Permission.GetPermissions;
using AZM.Abyan.Identity.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/realms/{realm}/clients/{clientId}/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public PermissionsController(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<List<PermissionResponse>>> GetPermissions(string realm, string clientId, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(clientId, out var clientIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidClientId"] });
            }

            var query = new GetPermissionsQuery
            {
                ClientId = clientIdGuid
            };

            var result = await _mediator.Send(query, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpGet("{permissionId}")]
    public async Task<ActionResult<PermissionResponse>> GetPermissionById(string realm, string clientId, string permissionId, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(permissionId, out var permissionIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidPermissionId"] });
            }

            var query = new GetPermissionByIdQuery
            {
                PermissionId = permissionIdGuid
            };

            var result = await _mediator.Send(query, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreatePermission(string realm, string clientId, [FromBody] CreatePermissionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // clientId parameter is the Keycloak client string ID (e.g., "formbuilder")
            // Command handler will create permission as role in Keycloak and save to database
            CreatePermissionCommand command = new CreatePermissionCommand
            {
                Name = request.Name,
                Description = request.Description,
                Controller = request.Controller,
                Action = request.Action,
                RealmName = realm,
                KeycloakClientId = clientId // Keycloak client string ID (e.g., "formbuilder")
            };

            var result = await _mediator.Send(command, cancellationToken);
            var response = StatusCode(result.StatusCode, result);
            return response;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpPut("{permissionId}")]
    public async Task<ActionResult> UpdatePermission(string realm, string clientId, string permissionId, [FromBody] UpdatePermissionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(permissionId, out var permissionIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidPermissionId"] });
            }

            var command = new UpdatePermissionCommand
            {
                PermissionId = permissionIdGuid,
                UpdatePermissionRequest = request,
                RealmName = realm,
                KeycloakClientId = clientId
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = _localizer["OperationFailed"] ?? ex.Message });
        }
    }

    [HttpDelete("{permissionId}")]
    public async Task<ActionResult> DeletePermission(string realm, string clientId, string permissionId, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(permissionId, out var permissionIdGuid))
            {
                return BadRequest(new { message = _localizer["InvalidPermissionId"] });
            }

            var command = new DeletePermissionCommand
            {
                PermissionId = permissionIdGuid
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

