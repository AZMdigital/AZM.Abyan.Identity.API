namespace AZM.Abyan.Identity.Application.DTOs.Clients;

public class CreateClientRequest
{
   // public string ClientId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid RealmId { get; set; }
    //public bool Enabled { get; set; } = true;
    //public string Protocol { get; set; } = "openid-connect";
    //public bool PublicClient { get; set; }
    //public bool BearerOnly { get; set; }
    //public bool ServiceAccountsEnabled { get; set; }
    //public List<string> RedirectUris { get; set; } = new();
    //public List<string> WebOrigins { get; set; } = new();
    //public bool AuthorizationServicesEnabled { get; set; }
}
