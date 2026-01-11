using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.Services;

public interface IRoleService
{
    Task<List<ClientRoleResponse>> GetClientRolesAsync(string realm, string clientId, CancellationToken cancellationToken = default);
    Task CreateClientRoleAsync(string realm, string clientId, CreateClientRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteClientRoleAsync(string realm, string clientId, string roleName, CancellationToken cancellationToken = default);
    Task AssignClientRoleToUserAsync(string realm, AssignRoleRequest request, CancellationToken cancellationToken = default);
    Task RemoveClientRoleFromUserAsync(string realm, AssignRoleRequest request, CancellationToken cancellationToken = default);
}

