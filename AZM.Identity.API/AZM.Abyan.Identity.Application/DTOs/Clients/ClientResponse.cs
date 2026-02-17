using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.Clients;

public class ClientResponse
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [JsonIgnore]
    public bool Enabled { get; set; }
    [JsonIgnore]
    public string Protocol { get; set; } = string.Empty;
    [JsonIgnore]
    public bool PublicClient { get; set; }
    [JsonIgnore]
    public bool BearerOnly { get; set; }
    public List<string> RedirectUris { get; set; } = new();
    [JsonIgnore]
    public List<string> WebOrigins { get; set; } = new();
    [JsonIgnore]
    public bool AuthorizationServicesEnabled { get; set; }
}
