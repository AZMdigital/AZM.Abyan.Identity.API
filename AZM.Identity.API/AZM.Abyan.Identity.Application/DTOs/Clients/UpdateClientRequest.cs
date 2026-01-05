namespace AZM.Abyan.Identity.Application.DTOs.Clients;

public class UpdateClientRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool ServiceAccountsEnabled { get; set; }
    public List<string> RedirectUris { get; set; } = new();
    public List<string> WebOrigins { get; set; } = new();
    public bool AuthorizationServicesEnabled { get; set; }
}
