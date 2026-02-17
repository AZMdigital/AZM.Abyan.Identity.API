namespace AZM.Abyan.Identity.Application.Models;

public class KeycloakConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string? ClientInternalId { get; set; } // Keycloak client internal UUID (from URL)
    public string? ClientSecret { get; set; }
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}

