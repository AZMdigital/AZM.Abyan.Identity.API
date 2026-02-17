using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Create;

public class CreatePermissionCommandHandler(
    IRepository<Domain.Entities.Permission, Guid> permissionRepository,
    IKeycloakService keycloakService,
    IStringLocalizer<SharedResource> localizer,
    IOptions<KeycloakConfigurations> keycloakConfigurations) : IRequestHandler<CreatePermissionCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Entities.Permission, Guid> _permissionRepository = permissionRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;
    private readonly KeycloakConfigurations _keycloakConfigurations = keycloakConfigurations.Value;

    public async Task<Result<Guid>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get admin token
            var adminToken = await _keycloakService.GetAdminTokenAsync(cancellationToken);

            // Resolve client internal ID from configuration
            //var clientInternalId = ResolveClientInternalId(request.RealmName, request.KeycloakClientId);
            //if (string.IsNullOrEmpty(clientInternalId))
            //{
            //    return Result<Guid>.Failure(_localizer["ClientNotFound"] ?? $"Client '{request.KeycloakClientId}' not found in configuration for realm '{request.RealmName}'");
            //}

            // Parse ClientInternalId to Guid for local ClientId
            //if (!Guid.TryParse(request.ClientId, out var clientIdGuid))
            //{
            //    return Result<Guid>.Failure(_localizer["InvalidClientInternalId"] ?? $"Invalid client internal ID format: {clientInternalId}");
            //}

            // Prepare role attributes for Keycloak
            // Controller is mandatory, Action is optional
            var attributes = new Dictionary<string, string[]>
            {
                ["Controller"] = new[] { request.Controller }
            };

            if (!string.IsNullOrEmpty(request.Action))
            {
                attributes["Action"] = new[] { request.Action };
            }

            // Create permission as a role in Keycloak with custom attributes
            var createRoleRequest = new DTOs.Roles.CreateClientRoleRequest
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Attributes = attributes
            };

            await _keycloakService.CreateClientRoleAsync(
                request.RealmName,
                request.ClientId.ToString(),
                createRoleRequest,
                adminToken,
                cancellationToken);

            // Get the created role from Keycloak to get its ID
            var keycloakRoles = await _keycloakService.GetClientRolesAsync(request.RealmName, request.ClientId.ToString(), adminToken, cancellationToken);
            var createdRole = keycloakRoles.FirstOrDefault(r => r.Name == request.Name);

            if (createdRole == null || string.IsNullOrEmpty(createdRole.Id) || !Guid.TryParse(createdRole.Id, out var keycloakRoleId))
            {
                return Result<Guid>.Failure(_localizer["FailedToCreatePermissionInKeycloak"] ?? "Failed to create permission in Keycloak or retrieve its ID");
            }

            // Create local entity with ID from Keycloak
            var permission = new Domain.Entities.Permission
            {
                Id = keycloakRoleId,
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Controller = request.Controller,
                Action = request.Action,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _permissionRepository.CreateAsync(permission, cancellationToken);
            await _permissionRepository.SaveChangesAsync(cancellationToken);

            var result = Result<Guid>.Created(permission.Id, _localizer["PermissionCreatedSuccessfully"] ?? "Permission created successfully");

            // DEBUG: Capture result details as JSON string for debugging
            var debugResultJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                StatusCode = result.StatusCode,
                IsSuccess = result.IsSuccess,
                Message = result.Message,
                Data = result.Data,
                Errors = result.Errors
            });
            var debugResultForInspector = debugResultJson; // Variable for debugger inspection

            return result;
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }

    private string? ResolveClientInternalId(string realm, string clientId)
    {
        // Find tenant configuration for the realm
        if (!_keycloakConfigurations.Tenants.TryGetValue(realm, out var tenantConfig))
        {
            return null;
        }

        // Check KeycloakFormbuilder
        if (tenantConfig.KeycloakFormbuilder.ClientId.Equals(clientId, StringComparison.OrdinalIgnoreCase))
        {
            return tenantConfig.KeycloakFormbuilder.ClientInternalId;
        }

        // Check KeycloakWorkflow
        if (tenantConfig.KeycloakWorkflow.ClientId.Equals(clientId, StringComparison.OrdinalIgnoreCase))
        {
            return tenantConfig.KeycloakWorkflow.ClientInternalId;
        }

        return null;
    }
}

