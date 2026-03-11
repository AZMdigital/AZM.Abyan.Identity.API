using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class PolicySyncService(
    IKeycloakService keycloakService,
    IRepository<Policy, Guid> policyRepository,
    IRepository<Role, Guid> roleRepository,
    IdentityDbContext dbContext) : IPolicySyncService
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IRepository<Policy, Guid> _policyRepository = policyRepository;
    private readonly IRepository<Role, Guid> _roleRepository = roleRepository;
    private readonly IdentityDbContext _dbContext = dbContext;

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
                if (string.IsNullOrEmpty(keycloakPolicy.Id) || !Guid.TryParse(keycloakPolicy.Id, out var keycloakPolicyId))
                {
                    result.Errors.Add($"Policy '{keycloakPolicy.Name}' has invalid or missing ID, skipping");
                    continue;
                }

                var localPolicy = localPolicies.FirstOrDefault(p => p.Id == keycloakPolicyId);

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
                            var roleId = rolesList.First().id;
                            if (Guid.TryParse(roleId, out var roleIdGuid))
                            {
                                role = allRoles.FirstOrDefault(r => r.Id == roleIdGuid);
                            }
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
                        Id = keycloakPolicyId,
                        Name = keycloakPolicy.Name,
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
                    localPolicy.RoleId = role.Id;
                    localPolicy.UpdatedAt = DateTime.UtcNow;
                    localPolicy.UpdatedBy = Guid.Empty;
                    _policyRepository.Update(localPolicy);
                    result.Updated++;
                }
            }

            // Delete policies that don't exist in Keycloak
            var keycloakPolicyIds = keycloakPolicies
                .Where(p => !string.IsNullOrEmpty(p.Id) && Guid.TryParse(p.Id, out _))
                .Select(p => Guid.Parse(p.Id!))
                .ToHashSet();
            var policiesToDelete = localPolicies
                .Where(p => !keycloakPolicyIds.Contains(p.Id))
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
        public string id { get; set; } = string.Empty;
        public bool required { get; set; }
    }
}

