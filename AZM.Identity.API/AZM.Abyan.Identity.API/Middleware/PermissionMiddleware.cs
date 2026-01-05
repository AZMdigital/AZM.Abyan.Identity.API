using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace AZM.Abyan.Identity.API.Middleware;

public class PermissionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        // Check for AllowAnonymous
        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor == null)
        {
            // Not a controller action (e.g. health check or static file), proceed
            await _next(context);
            return;
        }

        // Ensure user is authenticated first
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var controller = actionDescriptor.ControllerName.ToLower();
        var action = actionDescriptor.ActionName.ToLower();
        var permission = $"api:{controller}:{action}";

        if (!context.User.IsInRole(permission))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            // Optionally write response body
            // await context.Response.WriteAsync("Forbidden");
            return;
        }

        await _next(context);
    }
}
