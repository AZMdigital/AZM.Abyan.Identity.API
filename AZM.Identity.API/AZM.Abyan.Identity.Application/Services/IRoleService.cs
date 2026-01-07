using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.Services;

public interface IRoleService
{
    Task<List<ClientRoleResponse>> GetClientRolesAsync(string clientId, CancellationToken cancellationToken = default);
    Task CreateClientRoleAsync(string clientId, CreateClientRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteClientRoleAsync(string clientId, string roleName, CancellationToken cancellationToken = default);
    Task AssignClientRoleToUserAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);
    Task RemoveClientRoleFromUserAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);
}

