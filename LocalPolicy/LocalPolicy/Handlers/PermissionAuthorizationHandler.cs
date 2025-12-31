using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LocalPolicy.Handlers;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // Check if user has the required permission claim
        var permissionClaims = context.User.FindAll("permissions");
        
        foreach (var claim in permissionClaims)
        {
            if (string.IsNullOrEmpty(claim.Value))
                continue;

            // Handle comma-separated permissions or single permission
            var permissions = claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

            // Check if the required permission is in the list
            if (permissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

