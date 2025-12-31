using Microsoft.AspNetCore.Authorization;

namespace LocalPolicy.Attributes;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"Permission:{permission}";
    }
}

