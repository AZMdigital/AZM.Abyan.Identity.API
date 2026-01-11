using AZM.Abyan.Identity.Application.DTOs.AuthZ;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class PolicySyncService : IPolicySyncService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IRepository<Policy, Guid> _policyRepository;
    private readonly IRepository<Role, Guid> _roleRepository;
    private readonly IdentityDbContext _dbContext;

    public PolicySyncService(
        IKeycloakService keycloakService,
        IRepository<Policy, Guid> policyRepository,
        IRepository<Role, Guid> roleRepository,
        IdentityDbContext dbContext)
    {
        _keycloakService = keycloakService;
        _policyRepository = policyRepository;
        _roleRepository = roleRepository;
        _dbContext = dbContext;
    }

    public async Task<SyncEntityResult> SyncPoliciesAsync(string realm, string clientId, string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all policies from Keycloak
            List<AZM.Abyan.Identity.Application.DTOs.AuthZ.PolicyDto> keycloakPolicies;
            try
            {
                keycloakPolicies = await _keycloakService.GetAllPoliciesAsync(realm, clientId, adminToken, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                // Client doesn't support policies or Authorization Services not enabled - skip silently
                return result;
            }

            // Get all policies from local database
            var localPolicies = await _policyRepository.GetWhere().ToListAsync(cancellationToken);

            // Get all roles for matching
            var allRoles = await _roleRepository.GetWhere().ToListAsync(cancellationToken);

            // Process each Keycloak policy (only role policies)
            foreach (var keycloakPolicy in keycloakPolicies.Where(p => p.Type == "role"))
            {
                var keycloakPolicyIdGuid = Guid.TryParse(keycloakPolicy.Id, out var policyId) ? (Guid?)policyId : null;
                var localPolicy = localPolicies.FirstOrDefault(p => p.KeycloakPolicyId == keycloakPolicyIdGuid);

                // Extract role name from policy config
                Role? role = null;
                if (keycloakPolicy.Config != null && keycloakPolicy.Config.TryGetValue("roles", out var rolesValue))
                {
                    try
                    {
                        var rolesJson = rolesValue?.ToString() ?? "[]";
                        var rolesList = JsonSerializer.Deserialize<List<RoleConfig>>(rolesJson);
                        if (rolesList != null && rolesList.Any())
                        {
                            var roleId = rolesList.First().Id;
                            role = allRoles.FirstOrDefault(r => r.KeycloakRoleId?.ToString() == roleId);
                        }
                    }
                    catch
                    {
                        // Ignore parsing errors
                    }
                }

                // If no role found, skip this policy
                if (role == null)
                {
                    result.Errors.Add($"Policy '{keycloakPolicy.Name}' has no associated role, skipping");
                    continue;
                }

                if (localPolicy == null)
                {
                    // Create new policy
                    localPolicy = new Policy
                    {
                        Id = Guid.NewGuid(),
                        Name = keycloakPolicy.Name,
                        KeycloakPolicyId = keycloakPolicyIdGuid,
                        RoleId = role.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _policyRepository.CreateAsync(localPolicy, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing policy
                    localPolicy.Name = keycloakPolicy.Name;
                    localPolicy.KeycloakPolicyId = keycloakPolicyIdGuid;
                    localPolicy.RoleId = role.Id;
                    localPolicy.UpdatedAt = DateTime.UtcNow;
                    localPolicy.UpdatedBy = Guid.Empty;
                    _policyRepository.Update(localPolicy);
                    result.Updated++;
                }
            }

            // Delete policies that don't exist in Keycloak
            var keycloakPolicyIds = keycloakPolicies
                .Select(p => Guid.TryParse(p.Id, out var id) ? (Guid?)id : null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
            var policiesToDelete = localPolicies
                .Where(p => p.KeycloakPolicyId.HasValue && !keycloakPolicyIds.Contains(p.KeycloakPolicyId.Value))
                .ToList();

            foreach (var policyToDelete in policiesToDelete)
            {
                _dbContext.Policies.Remove(policyToDelete);
                result.Deleted++;
            }

            await _policyRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error syncing policies: {ex.Message}");
        }

        return result;
    }

    private class RoleConfig
    {
        public string Id { get; set; } = string.Empty;
        public bool Required { get; set; }
    }
}

