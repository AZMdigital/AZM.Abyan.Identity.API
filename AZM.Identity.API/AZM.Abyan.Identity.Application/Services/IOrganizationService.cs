using AZM.Abyan.Identity.Application.DTOs.Organizations;
using AZM.Abyan.Identity.Application.DTOs.Users;

namespace AZM.Abyan.Identity.Application.Services;

public interface IOrganizationService
{
    Task<List<OrganizationResponse>> GetOrganizationsAsync(string realm, CancellationToken cancellationToken = default);
    Task<OrganizationResponse?> GetOrganizationByIdAsync(string realm, string organizationId, CancellationToken cancellationToken = default);
    Task<string> CreateOrganizationAsync(string realm, CreateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task UpdateOrganizationAsync(string realm, string organizationId, UpdateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task DeleteOrganizationAsync(string realm, string organizationId, CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetOrganizationMembersAsync(string realm, string organizationId, CancellationToken cancellationToken = default);
    Task AddMemberToOrganizationAsync(string realm, string organizationId, string userId, CancellationToken cancellationToken = default);
    Task RemoveMemberFromOrganizationAsync(string realm, string organizationId, string memberId, CancellationToken cancellationToken = default);
}
