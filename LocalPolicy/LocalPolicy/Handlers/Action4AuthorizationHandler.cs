using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LocalPolicy.Handlers;

public class Action4AuthorizationHandler : AuthorizationHandler<Action4Requirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        Action4Requirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // Check if user has admin role
        if (context.User.IsInRole("admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user has action4 permission
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
            if (permissions.Contains("action4", StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}

public class Action4Requirement : IAuthorizationRequirement
{
}

