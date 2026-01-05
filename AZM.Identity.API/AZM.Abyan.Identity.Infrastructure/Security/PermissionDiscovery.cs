using AZM.Abyan.Identity.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace AZM.Abyan.Identity.Infrastructure.Security;

public static class PermissionDiscovery
{
    public static List<Permission> Discover(Assembly assembly)
    {
        var permissions = new List<Permission>();

        var controllers = assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        foreach (var controller in controllers)
        {
            var controllerName = controller.Name.Replace("Controller", "", StringComparison.OrdinalIgnoreCase).ToLower();

            // Look for Class-level Permission attribute
            var classPermission = controller.GetCustomAttribute<PermissionAttribute>();

            // Get public methods that are likely actions
            var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName &&
                            (m.GetCustomAttributes<HttpMethodAttribute>().Any() ||
                             m.ReturnType.IsAssignableTo(typeof(IActionResult)) ||
                             m.ReturnType.IsAssignableTo(typeof(Task<IActionResult>))));

            foreach (var method in methods)
            {
                // skip if AllowAnonymous is present
                if (method.GetCustomAttribute<AllowAnonymousAttribute>() != null)
                {
                    continue;
                }

                var methodPermission = method.GetCustomAttribute<PermissionAttribute>();

                string resource, action, description, name;

                if (methodPermission != null)
                {
                    resource = methodPermission.Resource;
                    action = methodPermission.Action;
                    description = methodPermission.Description;
                    name = $"api:{resource}:{action}";
                }
                else if (classPermission != null)
                {
                    resource = classPermission.Resource;
                    action = method.Name.ToLower();
                    description = classPermission.Description;
                    name = $"api:{resource}:{action}";
                }
                else
                {
                    // Default convention
                    resource = controllerName;
                    action = method.Name.ToLower();
                    description = $"Permission for {controllerName} {action}";
                    name = $"api:{resource}:{action}";
                }

                // Avoid duplicates
                if (permissions.Any(p => p.Name == name))
                {
                    continue;
                }

                permissions.Add(new Permission
                {
                    Name = name,
                    Resource = resource,
                    Action = action,
                    Description = description
                });
            }
        }

        return permissions;
    }
}
