namespace AZM.Abyan.Identity.Infrastructure.Security;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class PermissionAttribute(string resource, string action, string description = "") : Attribute
{
    public string Resource { get; } = resource;
    public string Action { get; } = action;
    public string Description { get; } = description;
}
