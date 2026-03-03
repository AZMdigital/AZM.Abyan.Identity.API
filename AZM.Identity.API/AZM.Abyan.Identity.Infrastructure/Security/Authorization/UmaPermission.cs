namespace AZM.Abyan.Identity.Infrastructure.Security.Authorization;

public readonly record struct UmaPermission(
string Tenant,
string Path,
string Method)
{
    public string ToKeycloakPermission()
        => $"{Path}#{Method}";
}
