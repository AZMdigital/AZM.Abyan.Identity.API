using AZM.Abyan.Identity.Application.DTOs.AuthZ;

namespace AZM.Abyan.Identity.Application.Services;

public interface IUserPermissionQueryService
{
    Task<List<PermissionDto>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<PolicyDto>> GetUserPoliciesAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<ResourceDto>> GetUserResourcesAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<ScopeDto>> GetUserScopesAsync(string userId, CancellationToken cancellationToken = default);
}
