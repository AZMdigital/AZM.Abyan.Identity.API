namespace AZM.Abyan.Identity.Application.Models;

public class KeycloakConfigurations
{
    public Dictionary<string, RealmConfiguration> Realms { get; set; } = new();
    public Dictionary<string, TenantConfiguration> Tenants { get; set; } = new();
}

public class RealmConfiguration
{
    public KeycloakConfiguration KeycloakAdmin { get; set; } = new();
}

public class TenantConfiguration
{
    public KeycloakConfiguration KeycloakFormbuilder { get; set; } = new();
    public KeycloakConfiguration KeycloakWorkflow { get; set; } = new();
}

