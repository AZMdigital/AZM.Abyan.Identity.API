using AZM.Abyan.Identity.Application.DTOs.Realms;
using AZM.Abyan.Identity.Application.DTOs.Roles;

namespace AZM.Abyan.Identity.Application.Services;

public interface IRealmAdminService
{
    Task<List<RealmResponse>> GetAllRealmsAsync(CancellationToken cancellationToken = default);
    Task<RealmResponse?> GetRealmByNameAsync(string realmName, CancellationToken cancellationToken = default);
    Task CreateRealmAsync(CreateRealmRequest request, CancellationToken cancellationToken = default);
    Task UpdateRealmAsync(string realmName, UpdateRealmRequest request, CancellationToken cancellationToken = default);
    Task UpdateRealmPasswordPolicyAsync(string realmName, UpdateRealmPasswordPolicyRequest request, CancellationToken cancellationToken = default);
    Task DeleteRealmAsync(string realmName, CancellationToken cancellationToken = default);
    Task<List<RealmRoleResponse>> GetRealmRolesAsync(string realm, CancellationToken cancellationToken = default);
    Task CreateRealmRoleAsync(CreateRealmRoleRequest request, CancellationToken cancellationToken = default);
    Task UpdateRealmRoleAsync(string realm, string roleName, UpdateRealmRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteRealmRoleAsync(string realm, string roleName, CancellationToken cancellationToken = default);
    Task AssignRealmRoleToUserAsync(AssignRealmRoleRequest request, CancellationToken cancellationToken = default);
    Task RemoveRealmRoleFromUserAsync(AssignRealmRoleRequest request, CancellationToken cancellationToken = default);
}
