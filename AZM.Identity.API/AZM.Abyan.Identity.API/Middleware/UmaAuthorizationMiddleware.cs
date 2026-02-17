using AZM.Abyan.Identity.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace AZM.Abyan.Identity.API.Middleware;

public sealed class UmaAuthorizationMiddleware(
  RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IUmaAuthorizationService authorizationService, ILogger<UmaAuthorizationMiddleware> logger)
    {
        var endpoint = context.GetEndpoint();


        // Log authentication state
        logger.LogInformation("User authenticated: {IsAuthenticated}",
            context.User.Identity?.IsAuthenticated);

        logger.LogInformation("User identity name: {Name}",
            context.User.Identity?.Name);

        logger.LogInformation("Headers: {Headers}",
            context.Request.Headers.Authorization.ToString());

        if (IsPublic(context))
        {
            await _next(context);
            return;
        }
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            logger.LogWarning("User is not authenticated");

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        var token = context.Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", string.Empty);

        var allowed = await authorizationService
            .IsAuthorizedAsync(context, token, context.RequestAborted);

        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await _next(context);
    }

    private static bool IsPublic(HttpContext context)
        => context.Request.Path.StartsWithSegments("/swagger")
        || context.Request.Path.StartsWithSegments("/health")
        || context.Request.Path.StartsWithSegments("/api/Auth");
}
